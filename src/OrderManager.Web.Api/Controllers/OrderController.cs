using Microsoft.AspNetCore.Mvc;
using OrderManager.Application.Dto;
using OrderManager.Application.Services;
using OrderManager.Domain.Entities.Orders;
using OrderManager.Domain.Queries;

namespace OrderManager.Api.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET /api/orders
        // Listagem com filtros
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] OrderQueryParams queryParams)
        {
            var result = await _orderService.QueryAsync(queryParams);
            return Ok(result);
        }

        // GET /api/orders/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order is null)
                return NotFound();

            return Ok(order);
        }

        // POST /api/orders
        // Criação com itens
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newOrderId = await _orderService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = newOrderId }, null);
        }

        // PUT /api/orders/{id}
        // Atualizar dados gerais do pedido
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _orderService.UpdateAsync(id, dto);
            if (updated is null)
                return NotFound();

            return Ok(updated);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateOrdersBatch(
            [FromBody] IEnumerable<UpdateOrderDto> ordersToUpdate,
            CancellationToken ct)
        {
            if (ordersToUpdate == null || !ordersToUpdate.Any())
                return BadRequest(new { error = "Lista de pedidos vazia." });

            try
            {
                await _orderService.UpdateOrdersBatchAsync(ordersToUpdate, ct);
                return Ok(new { message = $"{ordersToUpdate.Count()} pedidos processados." });
            }
            catch (Exception ex)
            {
                // opcional: log detalhado
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // PUT /api/orders/{id}/status
        // Atualização apenas do status
        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] OrderStatus status)
        {
            var result = await _orderService.UpdateStatusAsync(id, status);
            if (!result)
                return NotFound();

            return NoContent();
        }

        // DELETE /api/orders/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _orderService.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

        // POST /api/orders/{id}/cancel
        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var cancelled = await _orderService.CancelAsync(id, dto);
                if (!cancelled) return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
