/**
 * StripeController.cs
 * 
 * Handles Stripe payment integration including Connect accounts for lenders and customer checkout
 * sessions for renters. Provides endpoints for payment processing and webhook handling.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StuMoov.Models;
using StuMoov.Models.PaymentModel;
using StuMoov.Services.StripeService;
using StuMoov.Dao;
using StuMoov.Models.UserModel;
using Stripe;
using Stripe.Checkout;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.IO;
using System.Threading.Tasks;

namespace StuMoov.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeController : ControllerBase
    {
        private readonly StuMoov.Services.StripeService.StripeService _stripeService;  // Service for Stripe operations
        private readonly PaymentDao _paymentDao;                                       // Data access for payment records
        private readonly IConfiguration _configuration;                                // Application configuration
        private readonly ILogger<StripeController> _logger;                           // Logging service

        /// <summary>
        /// Initialize the StripeController with required services
        /// </summary>
        public StripeController(
            StuMoov.Services.StripeService.StripeService stripeService,
            PaymentDao paymentDao,
            IConfiguration configuration,
            ILogger<StripeController> logger)
        {
            _stripeService = stripeService;
            _paymentDao = paymentDao;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Creates a Stripe Connect account for a lender user
        /// </summary>
        /// <returns>New Stripe Connect account details</returns>
        /// <route>POST: api/stripe/connect/accounts</route>
        [HttpPost("connect/accounts")]
        [Authorize(Policy = "LenderOnly")]
        public async Task<IActionResult> CreateConnectAccount()
        {
            // Extract user ID from claims in the authorization token
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return BadRequest(new Response(400, "Invalid user ID in token", null));
            }

            StripeConnectAccount? stripeAccount = await _stripeService.CreateConnectAccountForLenderAsync(userId);
            if (stripeAccount == null)
            {
                return BadRequest(new Response(400, "Failed to create Stripe Connect account", null));
            }

            return Ok(new Response(200, "Stripe Connect account created", stripeAccount));
        }

        /// <summary>
        /// Generates an onboarding link for a lender to complete their Stripe Connect account setup
        /// </summary>
        /// <returns>URL for the onboarding process</returns>
        /// <route>GET: api/stripe/connect/accounts/onboarding-link</route>
        [HttpGet("connect/accounts/onboarding-link")]
        [Authorize(Policy = "LenderOnly")]
        public async Task<IActionResult> GetOnboardingLink()
        {
            // Extract user ID from claims in the authorization token
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return BadRequest(new Response(400, "Invalid user ID in token", null));
            }

            string? baseUrl = _configuration["AppBaseUrl"];
            string? refreshUrl = $"{baseUrl}/";
            string? returnUrl = $"{baseUrl}/";

            string? accountLinkUrl = await _stripeService.CreateOnboardingLinkForLenderAsync(userId, refreshUrl, returnUrl);
            if (accountLinkUrl == null)
            {
                return BadRequest(new Response(400, "Failed to create onboarding link", null));
            }

            return Ok(new Response(200, "Onboarding link created", new { url = accountLinkUrl }));
        }

        /// <summary>
        /// Retrieves the current status of a lender's Stripe Connect account
        /// </summary>
        /// <returns>Connect account status information</returns>
        /// <route>GET: api/stripe/connect/accounts/status</route>
        [HttpGet("connect/accounts/status")]
        [Authorize(Policy = "LenderOnly")]
        public async Task<IActionResult> GetAccountStatus()
        {
            // Extract user ID from claims in the authorization token
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return BadRequest(new Response(400, "Invalid user ID in token", null));
            }

            StripeConnectAccount? account = await _stripeService.UpdateAccountStatusAsync(userId);
            if (account == null)
            {
                return NotFound(new Response(404, "Stripe Connect account not found", null));
            }

            return Ok(new Response(200, "Account status retrieved", account));
        }

        /// <summary>
        /// Creates a Stripe customer record for a renter user
        /// </summary>
        /// <returns>New Stripe customer details</returns>
        /// <route>POST: api/stripe/customers</route>
        [HttpPost("customers")]
        [Authorize(Policy = "RenterOnly")]
        public async Task<IActionResult> CreateCustomer()
        {
            // Extract user ID from claims in the authorization token
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return BadRequest(new Response(400, "Invalid user ID in token", null));
            }

            StripeCustomer? customer = await _stripeService.CreateCustomerForRenterAsync(userId);
            if (customer == null)
            {
                return BadRequest(new Response(400, "Failed to create Stripe customer", null));
            }

            return Ok(new Response(200, "Stripe customer created", customer));
        }

        /// <summary>
        /// Creates a checkout session for a renter to make a payment to a lender
        /// </summary>
        /// <param name="request">Details for the checkout session including price and connected account</param>
        /// <returns>Checkout session ID and redirect URL</returns>
        /// <route>POST: api/stripe/checkout/sessions</route>
        [HttpPost("checkout/sessions")]
        [Authorize(Policy = "RenterOnly")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.PriceId) || string.IsNullOrEmpty(request.ConnectedAccountId))
            {
                return BadRequest(new Response(400, "Invalid request parameters", null));
            }

            // Extract user ID from claims in the authorization token
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return BadRequest(new Response(400, "Invalid user ID in token", null));
            }

            // Make sure the user has a Stripe customer account
            // TODO: Create stripe customer account for all renters on registration
            StripeCustomer? customer = await _stripeService.CreateCustomerForRenterAsync(userId);
            if (customer == null || string.IsNullOrEmpty(customer.StripeCustomerId))
            {
                return BadRequest(new Response(400, "Failed to get or create Stripe customer", null));
            }

            string? baseUrl = _configuration["AppBaseUrl"];
            string? successUrl = $"{baseUrl}/payment/success?session_id={{CHECKOUT_SESSION_ID}}";
            string? cancelUrl = $"{baseUrl}/payment/cancel";

            // Calculate application fee (our fee)
            decimal applicationFeePercent = _configuration.GetValue<decimal>("Stripe:ApplicationFeePercent", 3m); // Default to 3%
            long applicationFeeAmount = (long)(request.Amount * (applicationFeePercent / 100m));

            try
            {
                Session session = await _stripeService.CreateCheckoutSessionAsync(
                    customer.StripeCustomerId,
                    request.PriceId,
                    successUrl,
                    cancelUrl,
                    request.ConnectedAccountId,
                    applicationFeeAmount);

                return Ok(new Response(200, "Checkout session created", new { Id = session.Id, Url = session.Url }));
            }
            catch (StripeException ex)
            {
                return BadRequest(new Response(400, $"Stripe error: {ex.Message}", null));
            }
        }

        /// <summary>
        /// Handles Stripe webhook events for payment and account status updates
        /// </summary>
        /// <returns>HTTP 200 OK if the webhook was processed successfully</returns>
        /// <route>POST: api/stripe/webhook</route>
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleWebhook()
        {
            string json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            string? webhookSecret = _configuration["Stripe:WebhookSecret"];
            string? signatureHeader = Request.Headers["Stripe-Signature"];

            if (string.IsNullOrEmpty(webhookSecret))
            {
                _logger.LogError("Stripe webhook secret is not configured.");
                return BadRequest(new Response(400, "Webhook secret not configured", null));
            }
            if (string.IsNullOrEmpty(signatureHeader))
            {
                _logger.LogWarning("Webhook request missing Stripe-Signature header.");
                return BadRequest(new Response(400, "Missing Stripe signature", null));
            }

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, webhookSecret);
                _logger.LogInformation($"Webhook received: {stripeEvent.Id}, Type: {stripeEvent.Type}");
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, $"Webhook signature verification failed: {ex.Message}");
                return BadRequest(new Response(400, $"Webhook error: {ex.Message}", null));
            }

            // Handle each webhook event
            try
            {
                switch (stripeEvent.Type)
                {
                    case "account.updated":
                        await HandleAccountUpdatedEvent(stripeEvent);
                        break;
                    case "payment_intent.succeeded":
                        await HandlePaymentIntentSucceededEvent(stripeEvent);
                        break;

                    // --- Invoice Events --- 
                    case "invoice.paid":
                        await HandleInvoicePaidAsync(stripeEvent);
                        break;
                    case "invoice.payment_failed":
                        await HandleInvoicePaymentFailedAsync(stripeEvent);
                        break;
                    case "invoice.voided":
                        await HandleInvoiceVoidedAsync(stripeEvent);
                        break;
                    case "invoice.marked_uncollectible":
                        await HandleInvoicePaymentFailedAsync(stripeEvent, isUncollectible: true);
                        break;

                    default:
                        _logger.LogInformation($"Unhandled webhook event type: {stripeEvent.Type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing webhook event {stripeEvent.Id} (Type: {stripeEvent.Type}): {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new Response(500, "Internal server error processing webhook", null));
            }

            return Ok();
        }

        /// <summary>
        /// Processes Stripe Connect account update events from webhooks
        /// </summary>
        /// <param name="stripeEvent">The webhook event containing account data</param>
        private async Task HandleAccountUpdatedEvent(Event stripeEvent)
        {
            if (stripeEvent.Data.Object is not Account account) return;
            _logger.LogInformation($"Processing account.updated event for Account: {account.Id}");
            await _stripeService.UpdateAccountFromWebhookAsync(account);
        }

        /// <summary>
        /// Processes successful payment intent events from webhooks
        /// </summary>
        /// <param name="stripeEvent">The webhook event containing payment intent data</param>
        private Task HandlePaymentIntentSucceededEvent(Event stripeEvent)
        {
            if (stripeEvent.Data.Object is not PaymentIntent paymentIntent) return Task.CompletedTask;
            _logger.LogInformation($"Processing payment_intent.succeeded event for PaymentIntent: {paymentIntent.Id}");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Processes paid invoice events from webhooks and updates payment records accordingly
        /// </summary>
        /// <param name="stripeEvent">The webhook event containing invoice data</param>
        private async Task HandleInvoicePaidAsync(Event stripeEvent)
        {
            if (stripeEvent.Data.Object is not Invoice invoice) return;
            _logger.LogInformation($"Processing invoice.paid event for Invoice: {invoice.Id}");

            Payment? payment = await _paymentDao.GetByStripeInvoiceIdAsync(invoice.Id);
            if (payment == null)
            {
                _logger.LogWarning($"Received invoice.paid webhook for Invoice {invoice.Id}, but no matching Payment record found.");
                return;
            }

            // Update the status
            if (payment.Status != PaymentStatus.PAID)
            {
                Payment? updatedPayment = await _paymentDao.UpdatePaymentStatusAsync(payment.Id, PaymentStatus.PAID);
                if (updatedPayment != null)
                {
                    _logger.LogInformation($"Updated Payment record {payment.Id} status to PAID for Invoice {invoice.Id}.");
                    // TODO: Potentially trigger other actions (e.g., notify user, finalize booking steps)
                }
                else
                {
                    _logger.LogError($"Failed to update Payment record {payment.Id} status to PAID for Invoice {invoice.Id}.");
                }
            }
            else
            {
                _logger.LogInformation($"Payment record {payment.Id} already marked as PAID for Invoice {invoice.Id}. No status change needed.");
            }
        }

        /// <summary>
        /// Processes failed or uncollectible invoice events from webhooks and updates payment records
        /// </summary>
        /// <param name="stripeEvent">The webhook event containing invoice data</param>
        /// <param name="isUncollectible">Flag indicating if the invoice is marked as uncollectible</param>
        private async Task HandleInvoicePaymentFailedAsync(Event stripeEvent, bool isUncollectible = false)
        {
            if (stripeEvent.Data.Object is not Invoice invoice) return;
            string eventType = isUncollectible ? "invoice.marked_uncollectible" : "invoice.payment_failed";
            _logger.LogInformation($"Processing {eventType} event for Invoice: {invoice.Id}");

            Payment? payment = await _paymentDao.GetByStripeInvoiceIdAsync(invoice.Id);
            if (payment == null)
            {
                _logger.LogWarning($"Received {eventType} webhook for Invoice {invoice.Id}, but no matching Payment record found.");
                return;
            }

            // Update status to UNCOLLECTIBLE
            PaymentStatus targetStatus = PaymentStatus.UNCOLLECTIBLE;
            if (payment.Status != targetStatus)
            {
                Payment? updatedPayment = await _paymentDao.UpdatePaymentStatusAsync(payment.Id, targetStatus);
                if (updatedPayment != null)
                {
                    _logger.LogInformation($"Updated Payment record {payment.Id} status to {targetStatus} for Invoice {invoice.Id}.");
                    // TODO: Could notify the renter or admin here
                }
                else
                {
                    _logger.LogError($"Failed to update Payment record {payment.Id} status to {targetStatus} for Invoice {invoice.Id}.");
                }
            }
            else
            {
                _logger.LogInformation($"Payment record {payment.Id} already marked as {targetStatus} for Invoice {invoice.Id}. No status change needed.");
            }
        }

        /// <summary>
        /// Processes voided invoice events from webhooks and updates payment records
        /// </summary>
        /// <param name="stripeEvent">The webhook event containing invoice data</param>
        private async Task HandleInvoiceVoidedAsync(Event stripeEvent)
        {
            if (stripeEvent.Data.Object is not Invoice invoice) return;
            _logger.LogInformation($"Processing invoice.voided event for Invoice: {invoice.Id}");

            Payment? payment = await _paymentDao.GetByStripeInvoiceIdAsync(invoice.Id);
            if (payment == null)
            {
                _logger.LogWarning($"Received invoice.voided webhook for Invoice {invoice.Id}, but no matching Payment record found.");
                return;
            }

            // Update status to VOID
            if (payment.Status != PaymentStatus.VOID)
            {
                Payment? updatedPayment = await _paymentDao.UpdatePaymentStatusAsync(payment.Id, PaymentStatus.VOID);
                if (updatedPayment != null)
                {
                    _logger.LogInformation($"Updated Payment record {payment.Id} status to VOID for Invoice {invoice.Id}.");
                }
                else
                {
                    _logger.LogError($"Failed to update Payment record {payment.Id} status to VOID for Invoice {invoice.Id}.");
                }
            }
            else
            {
                _logger.LogInformation($"Payment record {payment.Id} already marked as VOID for Invoice {invoice.Id}. No status change needed.");
            }
        }
    }

    /// <summary>
    /// Request model for creating a checkout session
    /// </summary>
    public class CreateCheckoutSessionRequest
    {
        public string PriceId { get; set; }          // ID of the price object in Stripe
        public string ConnectedAccountId { get; set; } // ID of the connected account (lender) to receive payment
        public decimal Amount { get; set; }           // Total amount to charge in smallest currency unit
    }
}