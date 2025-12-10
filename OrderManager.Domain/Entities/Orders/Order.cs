using OrderManager.Domain.Enums;
using OrderManager.Domain.Shared;

namespace OrderManager.Domain.Entities.Orders
{
    public class Order : EntityBase
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = default!;
        public string CustomerEmail { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Total { get; set; }

        public Address ShippingAddress { get; set; } = null!;

        // Concurrency token para Postgres
        public uint xmin { get; set; } // Usado pelo Postgres automaticamente

        public List<OrderItem> Items { get; set; } = new();

        // Status de pagamento
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        // Método de domínio para atualizar o status
        public void UpdatePaymentStatus(PaymentStatus newStatus)
        {
            PaymentStatus = newStatus;
        }

        //Para calculo do frete
        public decimal FreightCost { get; set; }
        public FreightType FreightType { get; set; }
        public int EstimatedDeliveryDays { get; set; }

        public void SetFreight(decimal cost, FreightType type, int estimatedDays)
        {
            if (cost < 0)
                throw new ArgumentException("O valor do frete não pode ser negativo.", nameof(cost));

            FreightCost = cost;
            FreightType = type;
            EstimatedDeliveryDays = estimatedDays;
        }

        // Método usado ao reconstruir o pedido a partir do banco
        public void SetItems(List<OrderItem> items)
        {
            Items.Clear();
            if (items != null && items.Count > 0)
                Items.AddRange(items);
        }

        public void AddItem(OrderItem item)
        {
            Items.Add(item);
        }

        public void RecalculateTotal()
        {
            Total = Items.Sum(i => i.Quantity * i.UnitPrice);
        }

        public void UpdateStatus(OrderStatus newStatus)
        {
            Status = newStatus;
        }

        public void Cancel(string reason)
        {
            Status = OrderStatus.Cancelled;
        }

        public void ClearItems()
        {
            Items.Clear();
        }

        public void UpdateCustomerName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) throw new ArgumentException("...");
            CustomerName = newName;
        }

        public void SetShippingAddress(Address address)
        {
            ShippingAddress = address ?? throw new ArgumentNullException(nameof(address));
        }

        public void SetItems(IEnumerable<OrderItem> items)
        {
            ClearItems();
            foreach (var item in items)
                AddItem(item);
        }
    }
}
