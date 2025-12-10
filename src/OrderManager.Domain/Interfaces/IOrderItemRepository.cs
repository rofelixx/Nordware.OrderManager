using OrderManager.Domain.Entities.Orders;

namespace OrderManager.Domain.Interfaces
{

    public interface IOrderItemRepository
    {
        Task<OrderItem?> GetByIdAsync(Guid id);
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId);

        Task AddAsync(OrderItem item);
        Task AddRangeAsync(IEnumerable<OrderItem> items);

        Task UpdateAsync(OrderItem item);
        Task DeleteAsync(Guid id);
    }
}
