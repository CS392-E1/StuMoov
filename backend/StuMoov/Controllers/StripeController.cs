using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StuMoov.Models;
using StuMoov.Services.StripeService;
using StuMoov.Dao;
using StuMoov.Models.UserModel;
using Stripe;
using Stripe.Checkout;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace StuMoov.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeController : ControllerBase
    {
        private readonly StripeService _stripeService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeService> _logger;

        public StripeController(
            StripeCustomerDao stripeCustomerDao,
            StripeConnectAccountDao stripeConnectAccountDao,
            UserDao userDao,
            IConfiguration configuration,
            ILogger<StripeService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _stripeService = new StripeService(configuration, stripeCustomerDao, stripeConnectAccountDao, userDao, _logger);
        }

        [HttpPost("connect/accounts")]
        [Authorize(Policy = "LenderOnly")]
        public async Task<IActionResult> CreateConnectAccount()
        {
            // Get user ID from claims
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

        [HttpGet("connect/accounts/onboarding-link")]
        public async Task<IActionResult> GetOnboardingLink()
        {
            // Get user ID from claims
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

        [HttpGet("connect/accounts/status")]
        [Authorize(Policy = "LenderOnly")]
        public async Task<IActionResult> GetAccountStatus()
        {
            // Get user ID from claims
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

        [HttpPost("customers")]
        [Authorize(Policy = "RenterOnly")]
        public async Task<IActionResult> CreateCustomer()
        {
            // Get user ID from claims
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

        [HttpPost("checkout/sessions")]
        [Authorize(Policy = "RenterOnly")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.PriceId) || string.IsNullOrEmpty(request.ConnectedAccountId))
            {
                return BadRequest(new Response(400, "Invalid request parameters", null));
            }

            // Get user ID from claims
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return BadRequest(new Response(400, "Invalid user ID in token", null));
            }

            // Make sure the user has a Stripe customer account
            StripeCustomer? customer = await _stripeService.CreateCustomerForRenterAsync(userId);
            if (customer == null || string.IsNullOrEmpty(customer.StripeCustomerId))
            {
                return BadRequest(new Response(400, "Failed to get or create Stripe customer", null));
            }

            string? baseUrl = _configuration["AppBaseUrl"];
            string? successUrl = $"{baseUrl}/payment/success?session_id={{CHECKOUT_SESSION_ID}}";
            string? cancelUrl = $"{baseUrl}/payment/cancel";

            // Calculate application fee (platform fee)
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

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleWebhook()
        {
            string json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            // Get webhook secret from configuration
            string? webhookSecret = _configuration["Stripe:WebhookSecret"];
            if (string.IsNullOrEmpty(webhookSecret))
            {
                return BadRequest(new Response(400, "Webhook secret not configured", null));
            }

            // Verify webhook signature
            string? signatureHeader = HttpContext.Request.Headers["Stripe-Signature"];
            if (string.IsNullOrEmpty(signatureHeader))
            {
                return BadRequest(new Response(400, "Missing Stripe signature", null));
            }

            try
            {
                Event stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signatureHeader,
                    webhookSecret
                );

                // Handle the event based on its type
                switch (stripeEvent.Type)
                {
                    case "account.updated":
                        await HandleAccountUpdatedEvent(stripeEvent);
                        break;

                    case "payment_intent.succeeded":
                        await HandlePaymentIntentSucceededEvent(stripeEvent);
                        break;

                    default:
                        // Unhandled event type
                        break;
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                return BadRequest(new Response(400, $"Webhook error: {ex.Message}", null));
            }
        }

        // Helper methods for webhook event handling
        private async Task HandleAccountUpdatedEvent(Event stripeEvent)
        {
            Account? account = stripeEvent.Data.Object as Account;
            if (account == null) return;

            // Use the StripeService to update account status
            // The service will handle finding and updating the account
            await _stripeService.UpdateAccountFromWebhookAsync(account);
        }

        private async Task HandlePaymentIntentSucceededEvent(Event stripeEvent)
        {
            PaymentIntent? paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            // TODO: Update payment entries in the db using the service
            // Process successful payment (update booking status, notify users, etc.)
            // await _stripeService.ProcessSuccessfulPaymentAsync(paymentIntent);
        }
    }

    // Request models
    public class CreateCheckoutSessionRequest
    {
        public string PriceId { get; set; }
        public string ConnectedAccountId { get; set; }
        public decimal Amount { get; set; }
    }
}