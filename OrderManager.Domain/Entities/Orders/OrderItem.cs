namespace OrderManager.Domain.Entities.Orders
{
    public class OrderItem
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string Sku { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal Total => UnitPrice * Quantity;

        // EF navigation
        public Order? Order { get; private set; }

        private OrderItem() { } // EF

        public OrderItem(string sku, string name, int quantity, decimal unitPrice)
        {
            if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("SKU é obrigatório", nameof(sku));
            if (quantity <= 0) throw new ArgumentException("Quantidade deve ser > 0", nameof(quantity));
            if (unitPrice <= 0) throw new ArgumentException("Preço unitário deve ser > 0", nameof(unitPrice));

            Id = Guid.NewGuid();
            Sku = sku;
            Name = name;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0) throw new ArgumentException("Quantidade deve ser > 0", nameof(newQuantity));
            Quantity = newQuantity;
        }

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice <= 0) throw new ArgumentException("Preço deve ser > 0", nameof(newPrice));
            UnitPrice = newPrice;
        }
    }

}
