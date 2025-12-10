using MassTransit;
using OrderManager.Application.Events;
using OrderManager.Infrastructure.Idempotency;

namespace OrderManager.Infrastructure.Messaging
{
    public class OrderCreatedEmailConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly IMessageDeduplicator _dedup;

        public OrderCreatedEmailConsumer(IMessageDeduplicator dedup)
        {
            _dedup = dedup;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var messageId = context.Message.OrderId.ToString();

            if (!await _dedup.MarkProcessedIfNotExistsAsync(messageId))
            {
                // já processado
                return;
            }

            // Simular envio de email (pode ser replace por IEmailSender)
            var msg = context.Message;
            Console.WriteLine($"[Email] Enviando email para pedido {msg.OrderId} - cliente {msg.CustomerId}");

            // Se quiser simular falha para DLQ:
            // throw new InvalidOperationException("Simulated failure");
        }
    }
}
