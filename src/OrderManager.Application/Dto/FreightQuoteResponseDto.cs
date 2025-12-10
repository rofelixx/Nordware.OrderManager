using OrderManager.Domain.Enums;

namespace OrderManager.Application.Dto
{
    public class FreightQuoteResponseDto
    {
        public decimal Price { get; set; }
        public int EstimatedDays { get; set; }
        public FreightType Type { get; set; }
    }
}
