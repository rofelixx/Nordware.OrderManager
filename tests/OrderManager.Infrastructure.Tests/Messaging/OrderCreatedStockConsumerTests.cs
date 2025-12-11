using System;
using System.Threading.Tasks;
using MassTransit;
using Moq;
using OrderManager.Application.Events;
using OrderManager.Infrastructure.Idempotency;
using OrderManager.Infrastructure.Messaging;
using Xunit;

namespace OrderManager.Infrastructure.Tests.Messaging
{
    public class OrderCreatedStockConsumerTests
    {
        private readonly Mock<IMessageDeduplicator> _dedupMock;
        private readonly Mock<ConsumeContext<OrderCreatedEvent>> _contextMock;
        private readonly OrderCreatedStockConsumer _consumer;

        public OrderCreatedStockConsumerTests()
        {
            _dedupMock = new Mock<IMessageDeduplicator>();
            _contextMock = new Mock<ConsumeContext<OrderCreatedEvent>>();

            _consumer = new OrderCreatedStockConsumer(_dedupMock.Object);
        }

        [Fact]
        public async Task Consume_Should_Update_Stock_When_First_Time()
        {
            // Arrange
            var evt = new OrderCreatedEvent
            {
                OrderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                CustomerEmail = "email@email.com"
            };

            _contextMock.Setup(c => c.Message).Returns(evt);
            _dedupMock.Setup(x => x.MarkProcessedIfNotExistsAsync(evt.OrderId.ToString()))
                      .ReturnsAsync(true);

            // Act
            await _consumer.Consume(_contextMock.Object);

            // Assert
            _dedupMock.Verify(x => x.MarkProcessedIfNotExistsAsync(evt.OrderId.ToString()), Times.Once);
        }

        [Fact]
        public async Task Consume_Should_Not_Update_Stock_When_Duplicated()
        {
            // Arrange
            var evt = new OrderCreatedEvent
            {
                OrderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                CustomerEmail = "email@email.com"
            };

            _contextMock.Setup(x => x.Message).Returns(evt);

            _dedupMock.Setup(x => x.MarkProcessedIfNotExistsAsync(evt.OrderId.ToString()))
                      .ReturnsAsync(false); // mensagem duplicada

            // Act
            await _consumer.Consume(_contextMock.Object);

            // Assert
            _dedupMock.Verify(x => x.MarkProcessedIfNotExistsAsync(evt.OrderId.ToString()), Times.Once);

            // nenhuma outra ação deve ser executada
        }
    }
}
