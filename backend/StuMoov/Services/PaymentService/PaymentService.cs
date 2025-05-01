/**
 * PaymentService.cs
 *
 * Manages payment-related operations for the StuMoov application.
 * Provides functionality to retrieve the Stripe invoice URL for a specific payment.
 * Integrates with the PaymentDao for data access and the Stripe InvoiceService for invoice operations.
 */

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StuMoov.Dao;
using StuMoov.Models;
using Stripe;
using System;
using System.Threading.Tasks;

namespace StuMoov.Services.PaymentService
{
    /// <summary>
    /// Service responsible for managing payment operations, specifically retrieving Stripe invoice URLs.
    /// </summary>
    public class PaymentService
    {
        /// <summary>
        /// Data access object for payment-related database operations.
        /// </summary>
        private readonly PaymentDao _paymentDao;

        /// <summary>
        /// Stripe service for handling invoice operations.
        /// </summary>
        private readonly InvoiceService _invoiceService;

        /// <summary>
        /// Logger for recording payment-related events and errors.
        /// </summary>
        private readonly ILogger<PaymentService> _logger;

        /// <summary>
        /// Initializes a new instance of the PaymentService with required dependencies.
        /// </summary>
        /// <param name="paymentDao">DAO for payment operations.</param>
        /// <param name="logger">Logger for diagnostics.</param>
        public PaymentService(PaymentDao paymentDao, ILogger<PaymentService> logger)
        {
            _paymentDao = paymentDao;
            _invoiceService = new InvoiceService();
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the Stripe hosted invoice URL for a specific payment.
        /// </summary>
        /// <param name="paymentId">The ID of the payment.</param>
        /// <returns>A Response object with the status, message, and invoice URL, if available.</returns>
        public async Task<Response> GetInvoiceUrlAsync(Guid paymentId)
        {
            try
            {
                _logger.LogInformation($"PaymentService: Attempting to get invoice URL for Payment ID: {paymentId}");

                // Fetch payment record
                var payment = await _paymentDao.GetByIdAsync(paymentId);
                if (payment == null)
                {
                    _logger.LogWarning($"PaymentService: Payment with ID {paymentId} not found.");
                    return new Response(StatusCodes.Status404NotFound, "Payment record not found.", null);
                }

                // Validate Stripe invoice ID
                if (string.IsNullOrEmpty(payment.StripeInvoiceId))
                {
                    _logger.LogWarning($"PaymentService: Payment {paymentId} does not have a Stripe Invoice ID associated.");
                    return new Response(StatusCodes.Status400BadRequest, "No Stripe invoice associated with this payment.", null);
                }

                // Fetch Stripe invoice
                Invoice stripeInvoice;
                try
                {
                    _logger.LogInformation($"PaymentService: Fetching Stripe Invoice ID: {payment.StripeInvoiceId}");
                    stripeInvoice = await _invoiceService.GetAsync(payment.StripeInvoiceId);
                }
                catch (StripeException ex)
                {
                    _logger.LogError(ex, $"PaymentService: Stripe API error fetching invoice {payment.StripeInvoiceId}: {ex.StripeError?.Message ?? ex.Message}");
                    return new Response(StatusCodes.Status502BadGateway, "Failed to retrieve invoice details from Stripe.", null);
                }

                // Validate invoice URL
                if (stripeInvoice == null || string.IsNullOrEmpty(stripeInvoice.HostedInvoiceUrl))
                {
                    _logger.LogWarning($"PaymentService: Stripe Invoice {payment.StripeInvoiceId} retrieved but missing HostedInvoiceUrl.");
                    return new Response(StatusCodes.Status404NotFound, "Hosted invoice URL not available for this payment.", null);
                }

                _logger.LogInformation($"PaymentService: Successfully retrieved HostedInvoiceUrl for Payment ID {paymentId}.");
                return new Response(StatusCodes.Status200OK, "Invoice URL retrieved successfully.", stripeInvoice.HostedInvoiceUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PaymentService: Unexpected error retrieving invoice URL for Payment ID {paymentId}: {ex.Message}");
                return new Response(StatusCodes.Status500InternalServerError, "An internal error occurred.", null);
            }
        }
    }
}