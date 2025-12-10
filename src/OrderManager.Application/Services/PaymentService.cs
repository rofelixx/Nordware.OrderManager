using OrderManager.Application.Interfaces;
using OrderManager.Domain.Enums;
using OrderManager.Domain.Interfaces;

namespace OrderManager.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _uow;

        public PaymentService(IOrderRepository orderRepository, IUnitOfWork uow)
        {
            _orderRepository = orderRepository;
            _uow = uow;
        }

        public async Task HandlePaymentWebhookAsync(Guid orderId, string status, CancellationToken ct)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException("Order not found");

            if (!Enum.TryParse<PaymentStatus>(status, ignoreCase: true, out var parsedStatus))
                throw new ArgumentException($"Status de pagamento inválido: {status}");

            order.PaymentStatus = parsedStatus;

            await _orderRepository.UpdateAsync(order);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
