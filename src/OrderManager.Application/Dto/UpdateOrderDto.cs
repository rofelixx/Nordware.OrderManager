using OrderManager.Domain.Enums;

namespace OrderManager.Application.Dto
{
    public class UpdateOrderDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } 
        public string CustomerEmail { get; set; }
        public AddressDto ShippingAddress { get; set; } = null!;
        public List<UpdateOrderItemDto> Items { get; set; }
        public decimal FreightCost { get; set; }
        public FreightType FreightType { get; set; }
        public int EstimatedDeliveryDays { get; set; }
    }
}
