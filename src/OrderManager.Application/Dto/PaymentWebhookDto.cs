namespace OrderManager.Application.Dto
{
    public class PaymentWebhookDto
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; } = null!;
        public string PaymentProviderId { get; set; } = null!;
    }
}
