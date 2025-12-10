using Microsoft.EntityFrameworkCore;
using OrderManager.Domain.Entities.Orders;
using OrderManager.Domain.Enums;

namespace OrderManager.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Address> Addresses => Set<Address>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ORDER
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.CustomerId).IsRequired();
                entity.Property(o => o.CustomerName).HasMaxLength(100).IsRequired();
                entity.Property(o => o.CustomerEmail).HasMaxLength(150).IsRequired();
                entity.Property(o => o.Status).IsRequired();
                entity.Property(o => o.Total).HasPrecision(18, 2);
                entity.Property(o => o.FreightCost).HasPrecision(18, 2);
                entity.Property(o => o.FreightType).IsRequired();
                entity.Property(o => o.EstimatedDeliveryDays).IsRequired();
                entity.Property(o => o.PaymentStatus).IsRequired();

                entity.Property(o => o.xmin).IsRowVersion().IsConcurrencyToken();

                entity.HasMany(o => o.Items)
                      .WithOne(oi => oi.Order)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.OwnsOne(o => o.ShippingAddress, sa =>
                {
                    sa.Property(a => a.Cep).HasMaxLength(10).IsRequired();
                    sa.Property(a => a.Street).HasMaxLength(200).IsRequired();
                    sa.Property(a => a.Complement).HasMaxLength(100);
                    sa.Property(a => a.Neighborhood).HasMaxLength(100).IsRequired();
                    sa.Property(a => a.City).HasMaxLength(100).IsRequired();
                    sa.Property(a => a.State).HasMaxLength(2).IsRequired();
                });
            });

            // ORDER ITEM
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.Id);
                entity.Property(oi => oi.Sku).HasMaxLength(50).IsRequired();
                entity.Property(oi => oi.Name).HasMaxLength(150).IsRequired();
                entity.Property(oi => oi.Quantity).IsRequired();
                entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2).IsRequired();
            });

            // =========================
            // Seed de dados com IDs fixos
            // =========================

            var order1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var order2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var now = DateTime.SpecifyKind(new DateTime(2025, 12, 9, 12, 0, 0), DateTimeKind.Utc);

            // Orders
            modelBuilder.Entity<Order>().HasData(
                new Order
                {
                    Id = order1Id,
                    CustomerId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    CustomerName = "Joao do BV",
                    CustomerEmail = "joao.cwb@email.com",
                    Status = OrderStatus.Pending,
                    PaymentStatus = PaymentStatus.Pending,
                    Total = 200m,
                    FreightCost = 20m,
                    FreightType = FreightType.Standard,
                    EstimatedDeliveryDays = 5,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Order
                {
                    Id = order2Id,
                    CustomerId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    CustomerName = "Maria Souza",
                    CustomerEmail = "maria.souza@email.com",
                    Status = OrderStatus.Pending,
                    PaymentStatus = PaymentStatus.Pending,
                    Total = 90m,
                    FreightCost = 15m,
                    FreightType = FreightType.Standard,
                    EstimatedDeliveryDays = 3,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            );

            // ShippingAddress (Owned Entity)
            modelBuilder.Entity<Order>().OwnsOne(o => o.ShippingAddress).HasData(
                new
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    OrderId = order1Id,
                    Cep = "12345-678",
                    Street = "Rua das Flores",
                    Complement = "Apto 101",
                    Neighborhood = "Centro",
                    City = "Curitiba",
                    State = "PR"
                },
                new
                {
                    Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    OrderId = order2Id,
                    Cep = "98765-432",
                    Street = "Avenida Brasil",
                    Complement = "",
                    Neighborhood = "Jardins",
                    City = "Curitiba",
                    State = "PR"
                }
            );

            // OrderItems
            modelBuilder.Entity<OrderItem>().HasData(
                new
                {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                    OrderId = order1Id,
                    Sku = "SKU-001",
                    Name = "Produto A",
                    Quantity = 2,
                    UnitPrice = 50m
                },
                new
                {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                    OrderId = order1Id,
                    Sku = "SKU-002",
                    Name = "Produto B",
                    Quantity = 1,
                    UnitPrice = 100m
                },
                new
                {
                    Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                    OrderId = order2Id,
                    Sku = "SKU-003",
                    Name = "Produto C",
                    Quantity = 3,
                    UnitPrice = 30m
                }
            );
        }
    }
}
