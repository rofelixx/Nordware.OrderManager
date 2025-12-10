namespace OrderManager.Application.Events
{
    public class OrderCreatedEvent
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public IReadOnlyCollection<OrderCreatedItem> Items { get; set; } = Array.Empty<OrderCreatedItem>();
    }

    public class OrderCreatedItem
    {
        public Guid Id { get; set; }
        public string Sku { get; set; } = null!;
        public int Quantity { get; set; }
    }
}
