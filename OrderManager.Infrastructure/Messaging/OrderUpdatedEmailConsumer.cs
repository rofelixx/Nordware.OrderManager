using MassTransit;
using OrderManager.Application.Events;

namespace OrderManager.Infrastructure.Messaging
{
    public class OrderUpdatedEmailConsumer : IConsumer<OrderUpdatedEvent>
    {
        public async Task Consume(ConsumeContext<OrderUpdatedEvent> context)
        {
            var message = context.Message;

            Console.WriteLine($"[EMAIL] Pedido {message.OrderId} atualizado em {message.UpdatedAt}");

            await Task.CompletedTask;
        }
    }
}
