namespace OrderManager.Application.Dto
{
    public class PaymentStatusDto
    {
        public Guid OrderId { get; set; }

        /// <summary>
        /// Ex: "paid", "declined", "processing"
        /// </summary>
        public string Status { get; set; } = null!;

        /// <summary>
        /// Identificador da transação no provedor de pagamento
        /// </summary>
        public string TransactionId { get; set; } = null!;

        /// <summary>
        /// Valor pago (se aplicavel)
        /// </summary>
        public decimal? AmountPaid { get; set; }

        /// <summary>
        /// Mensagem adicional opcional do provedor
        /// </summary>
        public string? Message { get; set; }
    }
}
