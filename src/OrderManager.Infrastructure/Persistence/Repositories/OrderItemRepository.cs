using Microsoft.EntityFrameworkCore;
using OrderManager.Domain.Entities.Orders;
using OrderManager.Domain.Interfaces;
using OrderManager.Infrastructure.Persistence;

namespace OrderManager.Infrastructure.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly AppDbContext _context;

        public OrderItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrderItem?> GetByIdAsync(Guid id)
        {
            return await _context.OrderItems.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.OrderItems
                                 .Where(i => i.OrderId == orderId)
                                 .ToListAsync();
        }

        public async Task AddAsync(OrderItem item)
        {
            await _context.OrderItems.AddAsync(item);
        }

        public async Task AddRangeAsync(IEnumerable<OrderItem> items)
        {
            await _context.OrderItems.AddRangeAsync(items);
        }

        public Task UpdateAsync(OrderItem item)
        {
            _context.OrderItems.Update(item);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var item = await _context.OrderItems.FindAsync(id);
            if (item != null)
                _context.OrderItems.Remove(item);
        }
    }
}
