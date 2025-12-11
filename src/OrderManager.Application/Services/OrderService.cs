using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OrderManager.Application.Dto;
using OrderManager.Application.Events;
using OrderManager.Application.Interfaces;
using OrderManager.Domain.Common;
using OrderManager.Domain.Entities.Orders;
using OrderManager.Domain.Interfaces;
using OrderManager.Domain.Queries;
using System.Text.Json;

namespace OrderManager.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _uow;
        private readonly IValidator<CreateOrderDto> _createValidator;
        private readonly IValidator<UpdateOrderDto> _updateValidator;
        private readonly IPublishEndpoint _publisher;
        private readonly IViaCepService _viaCepService;
        private readonly IFreightService _freightService;
        private readonly IDistributedCache _redis;

        public OrderService(IUnitOfWork uow, IValidator<CreateOrderDto> createValidator, IValidator<UpdateOrderDto> updateValidator,
            IPublishEndpoint publish, IViaCepService viaCepService, IFreightService freightService, IDistributedCache redis)
        {
            _uow = uow;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _publisher = publish;
            _viaCepService = viaCepService;
            _freightService = freightService;
            _redis = redis;
        }

        public async Task<Guid> CreateAsync(CreateOrderDto dto, CancellationToken ct = default)
        {
            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var order = new Order()
            {
                CustomerId = dto.CustomerId,
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail
            };

            foreach (var itemDto in dto.Items)
            {
                var item = new OrderItem(
                    itemDto.Sku,
                    itemDto.Name,
                    itemDto.Quantity,
                    itemDto.UnitPrice);

                order.AddItem(item);
            }

            order.RecalculateTotal();

            // calcular frete
            var freightReq = new FreightQuoteRequestDto
            {
                CepDestino = dto.CustomerCep,
                WeightKg = 1.0m,
                VolumeM3 = 0.01m
            };
            var freight = await _freightService.GetFreightQuoteAsync(freightReq, ct);

            order.SetFreight(
                freight.Price,
                freight.Type,
                freight.EstimatedDays
            );

            //buscar info do endereco pelo cep (Api externa)
            var addressInfo = await _viaCepService.GetAddressByCepAsync(dto.CustomerCep, ct);

            if (addressInfo == null)
                throw new ArgumentException("CEP inválido.", nameof(dto.CustomerCep));

            var address = new Address(
                cep: addressInfo.Cep,
                street: addressInfo.Logradouro,
                complement: addressInfo.Complemento,
                neighborhood: addressInfo.Bairro,
                city: addressInfo.Localidade,
                state: addressInfo.Uf
            );

            order.SetShippingAddress(address);

            // Persistencia
            await _uow.Orders.AddAsync(order);
            await _uow.SaveChangesAsync(ct);

            //Publicar o evento de criaçao de order
            var integrationEvent = new OrderCreatedEvent
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                CustomerEmail = order.CustomerEmail,
                CustomerName = order.CustomerName,
                CreatedAt = order.CreatedAt,
                Items = order.Items
                    .Select(x => new OrderCreatedItem
                    {
                        Id = x.Id,
                        Sku = x.Sku,
                        Quantity = x.Quantity
                    })
                    .ToList()
            };

            await _publisher.Publish(integrationEvent);

            return order.Id;
        }

        public async Task<OrderDto?> UpdateAsync(Guid id, UpdateOrderDto dto, CancellationToken ct = default)
        {
            var validation = await _updateValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var order = await _uow.Orders.GetByIdAsync(id);
            if (order == null) return null;

            // Atualiza campos simples
            if (!string.IsNullOrWhiteSpace(dto.CustomerName))
            {
                order.UpdateCustomerName(dto.CustomerName);
            }

            var address = new Address(
                cep: dto.ShippingAddress.Cep,
                street: dto.ShippingAddress.Street,
                complement: dto.ShippingAddress.Complement,
                neighborhood: dto.ShippingAddress.Neighborhood,
                city: dto.ShippingAddress.City,
                state: dto.ShippingAddress.State
            );

            order.SetShippingAddress(address);

            // Substitui itens por completo
            order.ClearItems();
            foreach (var itemDto in dto.Items)
            {
                var item = new OrderItem(itemDto.Sku, itemDto.Name, itemDto.Quantity, itemDto.UnitPrice);
                order.AddItem(item);
            }

            order.RecalculateTotal();

            await _uow.Orders.UpdateAsync(order);
            await _uow.SaveChangesAsync(ct);

            //PUBLICA EVENTO DE UPDATE
            var updateEvent = new OrderUpdatedEvent(
                order.Id,
                order.CustomerId,
                order.Total,
                order.EstimatedDeliveryDays,
                order.FreightCost,
                order.FreightType,
                order.Items.Select(x => new OrderUpdatedEventItem(
                    x.Id,
                    x.Name,
                    x.Quantity,
                    x.UnitPrice
                )).ToList(),
                DateTime.UtcNow
            );

            await _publisher.Publish(updateEvent, ct);

            // Atualiza cache Redis
            await _redis.SetStringAsync(
                $"order:{order.Id}",
                JsonSerializer.Serialize(order),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }
            );

            return new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                ShippingAddress = new AddressDto
                {
                    Cep = order.ShippingAddress.Cep,
                    Street = order.ShippingAddress.Street,
                    Complement = order.ShippingAddress.Complement,
                    Neighborhood = order.ShippingAddress.Neighborhood,
                    City = order.ShippingAddress.City,
                    State = order.ShippingAddress.State
                },
                CreatedAt = order.CreatedAt,
                Status = order.Status.ToString(),
                Total = order.Total,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    Sku = i.Sku,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Total = i.Total
                }).ToList()
            };
        }

        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var order = await _uow.Orders.GetByIdAsync(id);

            if (order == null)
                return null;

            var items = await _uow.OrderItems.GetByOrderIdAsync(id);
            order.SetItems(items.ToList());

            return order;
        }

        public async Task<PagedResult<Order>> QueryAsync(OrderQueryParams qp, CancellationToken ct = default)
        {
            return await _uow.Orders.GetPagedAsync(qp, ct);
        }

        public async Task<bool> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken ct = default)
        {
            var order = await _uow.Orders.GetByIdAsync(id);
            if (order == null) return false;

            order.UpdateStatus(status);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> CancelAsync(Guid id, CancelOrderDto dto, CancellationToken ct = default)
        {
            var order = await _uow.Orders.GetByIdAsync(id);
            if (order == null)
                return false;

            // Aplica regra de negócio: só permite cancelar se não estiver finalizado
            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Pedido não pode ser cancelado.");

            order.Cancel(dto.Reason);

            await _uow.Orders.UpdateAsync(order);
            await _uow.SaveChangesAsync(ct);

            // Opcional: publicar evento OrderCancelled
            // if (_publisher != null) await _publisher.Publish(new OrderCancelledEvent(order.Id, dto.Reason));

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            // Busca o pedido pelo Id
            var order = await _uow.Orders.GetByIdAsync(id);
            if (order == null)
                return false;

            // Remove do repositório
            await _uow.Orders.DeleteAsync(id);

            // Salva alterações
            await _uow.SaveChangesAsync(ct);

            // Opcional: publicar evento OrderDeleted
            // if (_publisher != null) await _publisher.Publish(new OrderDeletedEvent(id));

            return true;
        }

        public async Task UpdateOrdersBatchAsync(IEnumerable<UpdateOrderDto> ordersToUpdate, CancellationToken ct = default)
        {
            await Parallel.ForEachAsync(ordersToUpdate, (Func<UpdateOrderDto, CancellationToken, ValueTask>)(async (dto, token) =>
            {
                try
                {
                    // Recupera pedido
                    var order = await _uow.Orders.GetByIdAsync(dto.Id);
                    if (order == null)
                        return;

                    // Atualiza campos simples
                    if (!string.IsNullOrWhiteSpace(dto.CustomerName))
                        order.UpdateCustomerName(dto.CustomerName);

                    var address = new Address(
                        cep: dto.ShippingAddress.Cep,
                        street: dto.ShippingAddress.Street,
                        complement: dto.ShippingAddress.Complement,
                        neighborhood: dto.ShippingAddress.Neighborhood,
                        city: dto.ShippingAddress.City,
                        state: dto.ShippingAddress.State
                    );

                    order.SetShippingAddress(address);

                    // Substitui itens
                    order.ClearItems();
                    foreach (var itemDto in dto.Items)
                    {
                        var item = new OrderItem(itemDto.Sku, itemDto.Name, itemDto.Quantity, itemDto.UnitPrice);
                        order.AddItem(item);
                    }

                    // Recalcula total
                    order.RecalculateTotal();

                    // Atualiza frete
                    order.SetFreight(dto.FreightCost, dto.FreightType, dto.EstimatedDeliveryDays);

                    // Atualiza no repositório
                    await _uow.Orders.UpdateAsync(order);
                    await _uow.SaveChangesAsync(token);

                    // Atualiza cache Redis
                    await _redis.SetStringAsync(
                        $"order:{order.Id}",
                        JsonSerializer.Serialize(order),
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                        });

                    // Publica evento de update
                    var updateEvent = new OrderUpdatedEvent(
                        order.Id,
                        order.CustomerId,
                        order.Total,
                        order.EstimatedDeliveryDays,
                        order.FreightCost,
                        order.FreightType,
                        Enumerable.Select<OrderItem, OrderUpdatedEventItem>(order.Items, (Func<OrderItem, OrderUpdatedEventItem>)(x => new OrderUpdatedEventItem(
                            x.Id,
                            x.Name,
                            x.Quantity,
                            x.UnitPrice
                        ))).ToList(),
                        DateTime.UtcNow
                    );

                    await _publisher.Publish(updateEvent, token);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Console.WriteLine($"Concorrência detectada para pedido {dto.Id}: {ex.Message}");
                    // opcional: retry ou log detalhado
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar pedido {dto.Id}: {ex.Message}");
                }
            }));
        }
    }
}
