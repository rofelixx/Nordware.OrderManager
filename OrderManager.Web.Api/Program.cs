using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrderManager.Application.Dto;
using OrderManager.Application.Events;
using OrderManager.Application.Interfaces;
using OrderManager.Application.Services;
using OrderManager.Application.Validators;
using OrderManager.Domain.Interfaces;
using OrderManager.Infrastructure.Helper;
using OrderManager.Infrastructure.Idempotency;
using OrderManager.Infrastructure.Integration;
using OrderManager.Infrastructure.Messaging;
using OrderManager.Infrastructure.Persistence;
using OrderManager.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// DATABASE (PostgreSQL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection") ??
        "Host=postgres;Port=5432;Database=ordermanager;Username=postgres;Password=postgres",
        b => b.MigrationsAssembly("OrderManager.Infrastructure")
    )
);

// DEPENDENCY INJECTION
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

// External services
builder.Services.AddHttpClient<IViaCepService, ViaCepService>()
    .AddPolicyHandler(PollyHelper.GetRetryPolicy())
    .AddPolicyHandler(PollyHelper.GetCircuitBreakerPolicy());

builder.Services.AddHttpClient<IFreightService, FreightService>()
    .AddPolicyHandler(PollyHelper.GetRetryPolicy())
    .AddPolicyHandler(PollyHelper.GetCircuitBreakerPolicy());

builder.Services.AddScoped<IPaymentService, PaymentService>();

// CONTROLLERS
builder.Services.AddControllers();

// SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Nordware Order API",
        Version = "v1",
        Description = "API de gerenciamento de pedidos - Nordware"
    });
});

// FLUENT VALIDATION
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IValidator<CreateOrderDto>, CreateOrderDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateOrderDto>, UpdateOrderDtoValidator>();


// CONFIG PARA RETORNAR ERRO 400 COM O FLUENT VALIDATION
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .Select(e => new
            {
                Field = e.Key,
                Errors = e.Value.Errors.Select(x => x.ErrorMessage).ToArray()
            });

        var problem = new
        {
            Title = "Validation Failed",
            Status = StatusCodes.Status400BadRequest,
            Errors = errors
        };

        return new BadRequestObjectResult(problem);
    };
});

//CONFIG DO MASS TRANSIT + RABBIT
builder.Services.AddSingleton<IMessageDeduplicator, InMemoryMessageDeduplicator>();
builder.Services.AddMassTransit(x =>
{
    // Consumers
    x.AddConsumer<OrderCreatedEmailConsumer>();
    x.AddConsumer<OrderCreatedStockConsumer>();
    x.AddConsumer<OrderUpdatedEmailConsumer>();
    x.AddConsumer<OrderUpdatedStockConsumer>();

    x.SetKebabCaseEndpointNameFormatter();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        // ORDER CREATED - EMAIL
        cfg.ReceiveEndpoint("order-created-email", e =>
        {
            e.ConfigureConsumer<OrderCreatedEmailConsumer>(context);

            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
            e.UseInMemoryOutbox();

            e.SetQueueArgument("x-dead-letter-exchange", "order-dlx");
            e.SetQueueArgument("x-dead-letter-routing-key", "order-created-email-dlq");

            e.PrefetchCount = 10;
        });

        // 
        // ORDER CREATED -STOCK
        cfg.ReceiveEndpoint("order-created-stock", e =>
        {
            e.ConfigureConsumer<OrderCreatedStockConsumer>(context);

            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(10)));
            e.UseInMemoryOutbox();

            e.SetQueueArgument("x-dead-letter-exchange", "order-dlx");
            e.SetQueueArgument("x-dead-letter-routing-key", "order-created-stock-dlq");

            e.PrefetchCount = 5;
        });

        // ORDER UPDATED - EMAIL
        cfg.ReceiveEndpoint("order-updated-email", e =>
        {
            e.ConfigureConsumer<OrderUpdatedEmailConsumer>(context);

            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
            e.UseInMemoryOutbox();

            e.SetQueueArgument("x-dead-letter-exchange", "order-dlx");
            e.SetQueueArgument("x-dead-letter-routing-key", "order-updated-email-dlq");
        });

        // ORDER UPDATED - STOCK
        cfg.ReceiveEndpoint("order-updated-stock", e =>
        {
            e.ConfigureConsumer<OrderUpdatedStockConsumer>(context);

            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(10)));
            e.UseInMemoryOutbox();

            e.SetQueueArgument("x-dead-letter-exchange", "order-dlx");
            e.SetQueueArgument("x-dead-letter-routing-key", "order-updated-stock-dlq");
        });

        // DEAD LETTER QUEUES
        // CREATED
        cfg.ReceiveEndpoint("order-created-email-dlq", e =>
        {
            e.Handler<OrderCreatedEvent>(ctx =>
            {
                Console.WriteLine($"[DLQ][EMAIL][CREATED] {ctx.Message.OrderId}");
                return Task.CompletedTask;
            });
        });

        cfg.ReceiveEndpoint("order-created-stock-dlq", e =>
        {
            e.Handler<OrderCreatedEvent>(ctx =>
            {
                Console.WriteLine($"[DLQ][STOCK][CREATED] {ctx.Message.OrderId}");
                return Task.CompletedTask;
            });
        });

        // UPDATED
        cfg.ReceiveEndpoint("order-updated-email-dlq", e =>
        {
            e.Handler<OrderUpdatedEvent>(ctx =>
            {
                Console.WriteLine($"[DLQ][EMAIL][UPDATED] {ctx.Message.OrderId}");
                return Task.CompletedTask;
            });
        });

        cfg.ReceiveEndpoint("order-updated-stock-dlq", e =>
        {
            e.Handler<OrderUpdatedEvent>(ctx =>
            {
                Console.WriteLine($"[DLQ][STOCK][UPDATED] {ctx.Message.OrderId}");
                return Task.CompletedTask;
            });
        });
    });
});

//CONFIG PARA REDIS
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "redis:6379";
    options.InstanceName = "orders:";
});

// BUILD APP
var app = builder.Build();

// MIGRATIONS AUTO
// *Ativa automaticamente migrations ao iniciar*
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // aplica migrations automaticamente
}

// MIDDLEWARES
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
