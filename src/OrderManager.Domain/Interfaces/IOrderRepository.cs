using OrderManager.Domain.Common;
using OrderManager.Domain.Entities.Orders;
using OrderManager.Domain.Queries;

namespace OrderManager.Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id);
        Task<IEnumerable<Order>> GetAllAsync();

        Task<(IEnumerable<Order> Items, int TotalCount)> GetByQueryAsync(OrderQueryParams queryParams);
        Task<PagedResult<Order>> GetPagedAsync(OrderQueryParams query,CancellationToken cancellationToken = default);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(Guid id);
    }
}
