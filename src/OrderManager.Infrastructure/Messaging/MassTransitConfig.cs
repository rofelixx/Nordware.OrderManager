using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderManager.Infrastructure.Idempotency;

namespace OrderManager.Infrastructure.Messaging
{
    public static class MassTransitConfig
    {
        public static IServiceCollection AddRabbitMqMassTransit(this IServiceCollection services, IConfiguration config)
        {
            var rabbitHost = config.GetValue<string>("RabbitMq:Host") ?? "localhost";
            var rabbitUser = config.GetValue<string>("RabbitMq:User") ?? "guest";
            var rabbitPass = config.GetValue<string>("RabbitMq:Pass") ?? "guest";

            services.AddSingleton<IMessageDeduplicator, InMemoryMessageDeduplicator>();

            services.AddMassTransit(x =>
            {
                // Consumers
                x.AddConsumer<OrderCreatedEmailConsumer>();
                x.AddConsumer<OrderCreatedStockConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitHost, "/", h =>
                    {
                        h.Username(rabbitUser);
                        h.Password(rabbitPass);
                    });

                    // Receive endpoint para emails
                    cfg.ReceiveEndpoint("order-created-email-queue", e =>
                    {
                        // Retry: exponencial (5 tentativas)
                        e.UseMessageRetry(r => r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2)));

                        // configurar DLX: encaminhar mensagens rejeitadas para exchange "order-created-dlx"
                        // definindo argumento da fila (x-dead-letter-exchange)
                        e.SetQueueArgument("x-dead-letter-exchange", "order-created-dlx");
                        e.ConfigureConsumer<OrderCreatedEmailConsumer>(context);
                    });

                    // Receive endpoint para estoque
                    cfg.ReceiveEndpoint("order-created-stock-queue", e =>
                    {
                        e.UseMessageRetry(r => r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2)));
                        e.SetQueueArgument("x-dead-letter-exchange", "order-created-dlx");
                        e.ConfigureConsumer<OrderCreatedStockConsumer>(context);
                    });

                    // Criar fila DLQ (binding para exchange order-created-dlx)
                    cfg.ReceiveEndpoint("order-created-dlq", e =>
                    {
                        // aqui você pode logar / persistir a mensagem para inspeção manual
                        e.Handler<byte[]>(async context =>
                        {
                            // fallback genérico: log
                            Console.WriteLine($"[DLQ] Mensagem recebida na DLQ: Exchange: {context.ReceiveContext.InputAddress}, BodyLength: {context.Message?.Length}");
                        });
                    });
                });
            });

            return services;
        }
    }
}

