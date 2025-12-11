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
    public class OrderCreatedEmailConsumerTests
    {
        private readonly Mock<IMessageDeduplicator> _dedupMock;
        private readonly Mock<ConsumeContext<OrderCreatedEvent>> _contextMock;
        private readonly OrderCreatedEmailConsumer _consumer;

        public OrderCreatedEmailConsumerTests()
        {
            _dedupMock = new Mock<IMessageDeduplicator>();
            _contextMock = new Mock<ConsumeContext<OrderCreatedEvent>>();

            _consumer = new OrderCreatedEmailConsumer(_dedupMock.Object);
        }

        [Fact]
        public async Task Consume_Should_Process_Message_When_First_Time()
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
                      .ReturnsAsync(true);

            // Act
            await _consumer.Consume(_contextMock.Object);

            // Assert
            _dedupMock.Verify(x => x.MarkProcessedIfNotExistsAsync(evt.OrderId.ToString()), Times.Once);
        }

        [Fact]
        public async Task Consume_Should_Ignore_When_Message_Duplicated()
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
        }
    }
}