using OrderManager.Application.Dto;
using OrderManager.Domain.Common;
using OrderManager.Domain.Entities.Orders;
using OrderManager.Domain.Queries;

namespace OrderManager.Application.Services
{
    public interface IOrderService
    {
        Task<Guid> CreateAsync(CreateOrderDto dto, CancellationToken ct = default);
        Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<PagedResult<Order>> QueryAsync(OrderQueryParams queryParams, CancellationToken ct = default);
        Task<bool> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken ct = default);
        Task<bool> CancelAsync(Guid id, CancelOrderDto dto, CancellationToken ct = default);
        Task<OrderDto?> UpdateAsync(Guid id, UpdateOrderDto dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
        Task UpdateOrdersBatchAsync(IEnumerable<UpdateOrderDto> ordersToUpdate, CancellationToken ct);
    }
}
