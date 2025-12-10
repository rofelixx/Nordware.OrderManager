using MassTransit;
using OrderManager.Application.Events;
using OrderManager.Infrastructure.Idempotency;

namespace OrderManager.Infrastructure.Messaging
{
    public class OrderCreatedStockConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly IMessageDeduplicator _dedup;
        // injetar repositório de estoque aqui

        public OrderCreatedStockConsumer(IMessageDeduplicator dedup)
        {
            _dedup = dedup;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var messageId = context.Message.OrderId.ToString();
            if (!await _dedup.MarkProcessedIfNotExistsAsync(messageId))
                return;

            // Atualizar estoque (simulado)
            Console.WriteLine($"[Stock] Atualizando estoque para pedido {context.Message.OrderId}");
            await Task.CompletedTask;
        }
    }
}
