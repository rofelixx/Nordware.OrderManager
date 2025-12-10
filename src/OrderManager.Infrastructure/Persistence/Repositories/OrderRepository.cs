using Microsoft.EntityFrameworkCore;
using OrderManager.Domain.Common;
using OrderManager.Domain.Entities.Orders;
using OrderManager.Domain.Interfaces;
using OrderManager.Domain.Queries;
using OrderManager.Infrastructure.Persistence;
using System.Linq;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders.ToListAsync();
    }

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetByQueryAsync(OrderQueryParams queryParams)
    {
        var query = ApplyFilters(_context.Orders.AsQueryable(), queryParams);

        var items = await query.ToListAsync();
        var total = items.Count;

        return (items, total);
    }

    public async Task<PagedResult<Order>> GetPagedAsync(
        OrderQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_context.Orders.AsQueryable(), queryParams);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((queryParams.Page.GetValueOrDefault() - 1) * queryParams.PageSize.GetValueOrDefault())
            .Take(queryParams.PageSize.GetValueOrDefault())
            .ToListAsync(cancellationToken);

        return new PagedResult<Order>
        {
            Items = items,
            TotalCount = totalCount,
            Page = queryParams.Page.GetValueOrDefault(),
            PageSize = queryParams.PageSize.GetValueOrDefault()
        };
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
    }

    public async Task DeleteAsync(Guid id)
    {
        var order = await GetByIdAsync(id);
        if (order != null)
            _context.Orders.Remove(order);
    }

    private IQueryable<Order> ApplyFilters(IQueryable<Order> query, OrderQueryParams qp)
    {
        if (!string.IsNullOrWhiteSpace(qp.Status))
        {
            if (Enum.TryParse<OrderStatus>(qp.Status, ignoreCase: true, out var statusEnum))
            {
                query = query.Where(o => o.Status == statusEnum);
            }
        }

        if (qp.StartDate.HasValue)
            query = query.Where(o => o.CreatedAt >= qp.StartDate);

        if (qp.EndDate.HasValue)
            query = query.Where(o => o.CreatedAt <= qp.EndDate);

        if (!string.IsNullOrWhiteSpace(qp.CustomerName))
            query = query.Where(o => o.CustomerName.Contains(qp.CustomerName));

        return query;
    }
}
