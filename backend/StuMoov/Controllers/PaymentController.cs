/**
 * PaymentController.cs
 * 
 * Handles payment-related functionality for renters including generating invoice URLs.
 * Restricted to users with the Renter role via authorization policy.
 */

using Microsoft.AspNetCore.Mvc;
using StuMoov.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using StuMoov.Services.PaymentService;

namespace StuMoov.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize(Policy = "RenterOnly")]  // Restricts access to users with Renter role only
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;  // Service for payment processing operations
        private readonly ILogger<PaymentController> _logger;  // Logging service

        /// <summary>
        /// Initialize the PaymentController with required services
        /// </summary>
        /// <param name="paymentService">Service for payment processing</param>
        /// <param name="logger">Logging service</param>
        public PaymentController(PaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a URL to view or pay an invoice for a specific payment
        /// </summary>
        /// <param name="paymentId">The unique identifier of the payment</param>
        /// <returns>URL to access the invoice</returns>
        /// <route>GET: api/payments/{paymentId}/invoice-url</route>
        [HttpGet("{paymentId}/invoice-url")]
        public async Task<ActionResult<Response>> GetInvoiceUrl(Guid paymentId)
        {
            // Log the incoming request
            _logger.LogInformation($"Controller: Received request for invoice URL for Payment ID: {paymentId}");

            // Request the invoice URL from the payment service
            Response response = await _paymentService.GetInvoiceUrlAsync(paymentId);

            // Log the response status
            _logger.LogInformation($"Controller: Returning status {response.Status} for Payment ID: {paymentId}");

            return StatusCode(response.Status, response);
        }
    }
}