namespace OrderManager.Application.Dto
{
    public class CreateOrderDto
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public AddressDto ShippingAddress { get; set; } = null!;
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}
