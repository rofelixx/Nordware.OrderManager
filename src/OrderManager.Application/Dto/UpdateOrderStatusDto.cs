using OrderManager.Domain.Entities.Orders;

namespace OrderManager.Application.Dto
{
    public class UpdateOrderStatusDto
    {
        public OrderStatus Status { get; set; }
    }
}
