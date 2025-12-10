using Microsoft.EntityFrameworkCore;
using OrderManager.Domain.Common;

namespace OrderManager.Infrastructure.Persistence
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _ctx;
        protected readonly DbSet<T> _dbSet;
        public Repository(AppDbContext ctx) { _ctx = ctx; _dbSet = ctx.Set<T>(); }

        public virtual async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public virtual async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);
        public virtual void Update(T entity) => _dbSet.Update(entity);
        public virtual void Remove(T entity) => _dbSet.Remove(entity);
        public virtual IQueryable<T> Query() => _dbSet.AsQueryable();
    }

}
