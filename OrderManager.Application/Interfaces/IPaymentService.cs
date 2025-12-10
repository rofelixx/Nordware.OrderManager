namespace OrderManager.Application.Interfaces
{
    public interface IPaymentService
    {
        /// <summary>
        /// Processa o webhook recebido do provedor de pagamentos
        /// (ex: confirmação, falha..)
        /// </summary>
        Task HandlePaymentWebhookAsync(Guid orderId, string status, CancellationToken ct);
    }
}
