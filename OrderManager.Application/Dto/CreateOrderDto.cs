namespace OrderManager.Application.Dto
{
    public class CreateOrderDto
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string CustomerCep { get; set; } = null!;
        public string? ShippingAddress { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}
