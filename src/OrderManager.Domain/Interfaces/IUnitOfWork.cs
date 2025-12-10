namespace OrderManager.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IOrderRepository Orders { get; }
        IOrderItemRepository OrderItems { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
