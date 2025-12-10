using OrderManager.Domain.Enums;

namespace OrderManager.Application.Events
{
    public record OrderUpdatedEvent(
        Guid OrderId,
        Guid CustomerId,
        decimal Total,
        int EstimatedDeliveryDays,
        decimal FreightCost,
        FreightType FreightType,
        List<OrderUpdatedEventItem> Items,
        DateTime UpdatedAt
    );

    public record OrderUpdatedEventItem(
        Guid ProductId,
        string Name,
        int Quantity,
        decimal UnitPrice
    );
}
