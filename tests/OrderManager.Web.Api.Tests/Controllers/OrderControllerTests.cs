using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderManager.Domain.Entities.Orders;
using OrderManager.Infrastructure.Persistence;

namespace OrderManager.Web.Api.Tests.Controllers
{
    public class OrderRepositoryTests
    {
        private readonly DbContextOptions<AppDbContext> _dbOptions;

        public OrderRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private AppDbContext CreateContext() => new AppDbContext(_dbOptions);

        [Fact]
        public async Task AddAsync_ShouldPersistOrder()
        {
            // Arrange
            var ctx = CreateContext();
            var repo = new OrderRepository(ctx);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                CustomerName = "João",
                CustomerEmail = "email@email.com",
                ShippingAddress = new Address
                {
                    Street = "Street 1",
                    City = "City",
                    State = "PR",
                    Cep = "12345620",
                    Complement = "Complement",
                    Neighborhood = "Neighborhood",
                    Number = 100
                },
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await repo.AddAsync(order);
            await ctx.SaveChangesAsync();

            // Assert
            var dbOrder = await ctx.Orders.FirstOrDefaultAsync(x => x.Id == order.Id);
            dbOrder.Should().NotBeNull();
            dbOrder!.CustomerName.Should().Be("João");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnOrder_WhenExists()
        {
            var ctx = CreateContext();
            var repo = new OrderRepository(ctx);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                CustomerEmail = "email@email.com",
                CustomerName = "Maria",
                ShippingAddress = new Address
                {
                    Street = "Street 1",
                    City = "City",
                    State = "PR",
                    Cep = "12345620",
                    Complement = "Complement",
                    Neighborhood = "Neighborhood",
                    Number = 100
                },
                CreatedAt = DateTime.UtcNow
            };

            ctx.Orders.Add(order);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdAsync(order.Id);

            // Assert
            found.Should().NotBeNull();
            found!.CustomerName.Should().Be("Maria");
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyOrder()
        {
            var ctx = CreateContext();
            var repo = new OrderRepository(ctx);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                CustomerName = "Ana",
                CustomerEmail = "email@email.com",
                ShippingAddress = new Address
                {
                    Street = "Street 1",
                    City = "City",
                    State = "PR",
                    Cep = "12345620",
                    Complement = "Complement",
                    Neighborhood = "Neighborhood",
                    Number = 100
                },
                CreatedAt = DateTime.UtcNow
            };

            ctx.Orders.Add(order);
            await ctx.SaveChangesAsync();

            // Act
            order.CustomerName = "Ana Atualizada";
            await repo.UpdateAsync(order);
            await ctx.SaveChangesAsync();

            // Assert
            var dbOrder = await ctx.Orders.FirstAsync(x => x.Id == order.Id);
            dbOrder.CustomerName.Should().Be("Ana Atualizada");
        }
    }
}
