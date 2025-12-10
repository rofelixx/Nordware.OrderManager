namespace OrderManager.Application.Dto
{
    public class CreateOrderItemDto
    {
        public string Sku { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
