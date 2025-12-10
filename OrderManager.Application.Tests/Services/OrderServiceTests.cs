using FluentAssertions;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using OrderManager.Application.Dto;
using OrderManager.Application.Events;
using OrderManager.Application.Interfaces;
using OrderManager.Application.Services;
using OrderManager.Domain.Entities.Orders;
using OrderManager.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OrderManager.Application.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IValidator<CreateOrderDto>> _createValidatorMock;
        private readonly Mock<IValidator<UpdateOrderDto>> _updateValidatorMock;
        private readonly Mock<IPublishEndpoint> _publisherMock;
        private readonly Mock<IViaCepService> _viaCepServiceMock;
        private readonly Mock<IFreightService> _freightServiceMock;
        private readonly Mock<IDistributedCache> _redisMock;
        private readonly OrderService _service;

        public OrderServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _createValidatorMock = new Mock<IValidator<CreateOrderDto>>();
            _updateValidatorMock = new Mock<IValidator<UpdateOrderDto>>();
            _publisherMock = new Mock<IPublishEndpoint>();
            _viaCepServiceMock = new Mock<IViaCepService>();
            _freightServiceMock = new Mock<IFreightService>();
            _redisMock = new Mock<IDistributedCache>();

            _service = new OrderService(
                _uowMock.Object,
                _createValidatorMock.Object,
                _updateValidatorMock.Object,
                _publisherMock.Object,
                _viaCepServiceMock.Object,
                _freightServiceMock.Object,
                _redisMock.Object
            );
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateOrderAndPublishEvent()
        {
            // Arrange
            var dto = new CreateOrderDto
            {
                CustomerId = Guid.NewGuid(),
                CustomerName = "John Doe",
                CustomerEmail = "john@test.com",
                CustomerCep = "12345678",
                Items = new List<CreateOrderItemDto>
                {
                    new() { Sku = "SKU1", Name = "Item 1", Quantity = 2, UnitPrice = 10 }
                }
            };

            _createValidatorMock
                .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _freightServiceMock
                .Setup(f => f.GetFreightQuoteAsync(It.IsAny<FreightQuoteRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FreightQuoteResponseDto { Price = 5, Type = Domain.Enums.FreightType.Standard, EstimatedDays = 3 });

            _viaCepServiceMock
                .Setup(v => v.GetAddressByCepAsync(dto.CustomerCep, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ViaCepResponseDto(
                    Cep: "12345-678",
                    Logradouro: "Rua Exemplo",
                    Complemento: "Apto 101",
                    Bairro: "Centro",
                    Localidade: "Curitiba",
                    Uf: "PR"
                ));

            var orderAdded = default(Order);
            _uowMock.Setup(u => u.Orders.AddAsync(It.IsAny<Order>())).Callback<Order>(o => orderAdded = o).Returns(Task.CompletedTask);
            _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            _publisherMock.Setup(p => p.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var orderId = await _service.CreateAsync(dto);

            // Assert
            orderAdded.Should().NotBeNull();
            orderAdded.Id.Should().Be(orderId);
            _publisherMock.Verify(p => p.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateOrderAndPublishEvent()
        {
            // Arrange
            var existingOrder = new Order
            {
                CustomerId = Guid.NewGuid(),
                CustomerName = "Old Name",
                CustomerEmail = "old@test.com",
                Items = new List<OrderItem>
                {
                    new OrderItem("SKU1", "Item 1", 1, 10)
                }
            };

            _uowMock.Setup(u => u.Orders.GetByIdAsync(existingOrder.Id)).ReturnsAsync(existingOrder);
            _updateValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateOrderDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            _uowMock.Setup(u => u.Orders.UpdateAsync(existingOrder)).Returns(Task.CompletedTask);
            _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            _redisMock
                .Setup(r => r.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()
                ))
                .Returns(Task.CompletedTask);

            _publisherMock.Setup(p => p.Publish(It.IsAny<OrderUpdatedEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var dto = new UpdateOrderDto
            {
                Id = existingOrder.Id,
                CustomerName = "New Name",
                ShippingAddress = new AddressDto
                {
                    Cep = "12345678",
                    Street = "Rua Teste",
                    Complement = "",
                    Neighborhood = "Centro",
                    City = "Cidade",
                    State = "PR"
                },
                Items = new List<UpdateOrderItemDto>
                {
                    new() { Sku = "SKU2", Name = "Item 2", Quantity = 2, UnitPrice = 20 }
                },
                FreightCost = 10,
                FreightType = Domain.Enums.FreightType.Standard,
                EstimatedDeliveryDays = 5
            };

            // Act
            var result = await _service.UpdateAsync(existingOrder.Id, dto);

            // Assert
            result.Should().NotBeNull();
            result!.CustomerName.Should().Be("New Name");
            result.Items.Should().HaveCount(1);
            _publisherMock.Verify(p => p.Publish(It.IsAny<OrderUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateOrdersBatchAsync_ShouldProcessMultipleOrdersInParallel()
        {
            // Arrange
            var order1 = new UpdateOrderDto
            {
                Id = Guid.NewGuid(),
                CustomerName = "Name1",
                ShippingAddress = new AddressDto { Cep = "11111", Street = "Rua1", Complement = "", Neighborhood = "Bairro1", City = "Cidade1", State = "PR" },
                Items = new List<UpdateOrderItemDto>(),
                FreightCost = 5,
                FreightType = Domain.Enums.FreightType.Standard,
                EstimatedDeliveryDays = 3
            };
            var order2 = new UpdateOrderDto
            {
                Id = Guid.NewGuid(),
                CustomerName = "Name2",
                ShippingAddress = new AddressDto { Cep = "22222", Street = "Rua2", Complement = "", Neighborhood = "Bairro2", City = "Cidade2", State = "PR" },
                Items = new List<UpdateOrderItemDto>(),
                FreightCost = 10,
                FreightType = Domain.Enums.FreightType.Express,
                EstimatedDeliveryDays = 1
            };

            _uowMock.Setup(u => u.Orders.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Guid id) =>
            {
                return new Order { CustomerId = Guid.NewGuid(), CustomerName = "Old", Items = new List<OrderItem>() };
            });

            _uowMock.Setup(u => u.Orders.UpdateAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            
            _redisMock
                .Setup(r => r.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()
                ))
                .Returns(Task.CompletedTask);

            _publisherMock.Setup(p => p.Publish(It.IsAny<OrderUpdatedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var orders = new List<UpdateOrderDto> { order1, order2 };

            // Act
            await _service.UpdateOrdersBatchAsync(orders);

            // Assert
            _publisherMock.Verify(p => p.Publish(It.IsAny<OrderUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _redisMock
                .Setup(r => r.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }
    }
}
