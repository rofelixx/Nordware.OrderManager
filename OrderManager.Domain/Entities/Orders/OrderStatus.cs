namespace OrderManager.Domain.Entities.Orders
{
    public enum OrderStatus
    {
        Pending = 0,   // criado, aguardando pagamento
        Paid = 1,      // pago
        Processing = 2,// sendo processado para envio
        Shipped = 3,   // enviado
        Cancelled = 4,
        Rejected = 5,
        Completed = 6
    }
}
