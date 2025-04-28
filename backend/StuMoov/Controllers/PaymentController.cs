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
    [Authorize(Policy = "RenterOnly")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(PaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        // GET: api/payments/{paymentId}/invoice-url
        [HttpGet("{paymentId}/invoice-url")]
        public async Task<ActionResult<Response>> GetInvoiceUrl(Guid paymentId)
        {
            _logger.LogInformation($"Controller: Received request for invoice URL for Payment ID: {paymentId}");
            Response response = await _paymentService.GetInvoiceUrlAsync(paymentId);
            _logger.LogInformation($"Controller: Returning status {response.Status} for Payment ID: {paymentId}");
            return StatusCode(response.Status, response);
        }
    }
}