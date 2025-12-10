using OrderManager.Domain.Interfaces;

namespace OrderManager.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IOrderRepository Orders { get; }
        public IOrderItemRepository OrderItems { get; }

        public UnitOfWork(
            AppDbContext context,
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository)
        {
            _context = context;
            Orders = orderRepository;
            OrderItems = orderItemRepository;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
