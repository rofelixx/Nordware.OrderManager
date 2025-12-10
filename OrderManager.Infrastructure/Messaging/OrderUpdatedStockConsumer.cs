using MassTransit;
using OrderManager.Application.Events;

namespace OrderManager.Infrastructure.Messaging
{
    public class OrderUpdatedStockConsumer : IConsumer<OrderUpdatedEvent>
    {
        public async Task Consume(ConsumeContext<OrderUpdatedEvent> context)
        {
            var msg = context.Message;

            Console.WriteLine($"[STOCK] Atualizando estoque devido à alteração do pedido {msg.OrderId}...");

            await Task.CompletedTask;
        }
    }
}
