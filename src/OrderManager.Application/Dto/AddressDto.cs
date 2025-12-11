namespace OrderManager.Application.Dto
{
    public class AddressDto
    {
        public string Cep { get; set; } = null!;
        public string Street { get; set; } = null!;
        public string? Complement { get; set; }
        public int? Number { get; set; }
        public string Neighborhood { get; set; } = null!;
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
    }
}
