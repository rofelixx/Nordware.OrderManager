namespace OrderManager.Application.Dto
{
    public class FreightQuoteRequestDto
    {
        public string CepDestino { get; set; } = null!;
        public decimal WeightKg { get; set; }
        public decimal VolumeM3 { get; set; }
    }
}
