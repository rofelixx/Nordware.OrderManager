namespace OrderManager.Application.Dto
{
    public class UpdateOrderItemDto
    {
        public Guid? Id { get; set; } // opcional: se quiser preservar ids existentes
        public string Sku { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
