using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManager.Domain.Entities.Orders;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerName)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.ShippingAddress)
               .HasMaxLength(500);

        builder.Property(x => x.Total)
               .HasColumnType("numeric(18,2)")
               .IsRequired();

        // Controle de concorrência do PostgreSQL (xmin)
        builder.Property<uint>("xmin")
               .IsRowVersion(); // EF entende como token de concorrência

        builder.Navigation(x => x.Items)
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany<OrderItem>("_items")
            .WithOne(i => i.Order!)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
