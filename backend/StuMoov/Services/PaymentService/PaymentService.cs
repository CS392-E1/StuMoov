using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StuMoov.Dao;
using StuMoov.Models;
using Stripe;
using System;
using System.Threading.Tasks;

namespace StuMoov.Services.PaymentService
{
    public class PaymentService
    {
        private readonly PaymentDao _paymentDao;
        private readonly InvoiceService _invoiceService;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(PaymentDao paymentDao, ILogger<PaymentService> logger)
        {
            _paymentDao = paymentDao;
            _invoiceService = new InvoiceService();
            _logger = logger;
        }

        public async Task<Response> GetInvoiceUrlAsync(Guid paymentId)
        {
            try
            {
                _logger.LogInformation($"PaymentService: Attempting to get invoice URL for Payment ID: {paymentId}");

                var payment = await _paymentDao.GetByIdAsync(paymentId);

                if (payment == null)
                {
                    _logger.LogWarning($"PaymentService: Payment with ID {paymentId} not found.");
                    return new Response(StatusCodes.Status404NotFound, "Payment record not found.", null);
                }

                if (string.IsNullOrEmpty(payment.StripeInvoiceId))
                {
                    _logger.LogWarning($"PaymentService: Payment {paymentId} does not have a Stripe Invoice ID associated.");
                    return new Response(StatusCodes.Status400BadRequest, "No Stripe invoice associated with this payment.", null);
                }

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
