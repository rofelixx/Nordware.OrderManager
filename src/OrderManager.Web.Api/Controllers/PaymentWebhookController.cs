using Microsoft.AspNetCore.Mvc;
using OrderManager.Application.Dto;
using OrderManager.Application.Interfaces;

namespace OrderManager.Web.Api.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class PaymentWebhookController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentWebhookController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("payment")]
        public async Task<IActionResult> PaymentUpdated([FromBody] PaymentWebhookDto dto, CancellationToken ct)
        {
            try
            {
                await _paymentService.HandlePaymentWebhookAsync(dto.OrderId, dto.Status, ct);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Order not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
