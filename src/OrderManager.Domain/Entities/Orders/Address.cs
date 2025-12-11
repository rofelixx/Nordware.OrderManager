namespace OrderManager.Domain.Entities.Orders
{
    public class Address
    {
        public Guid Id { get; set; }
        public string Cep { get; set; } = null!;
        public string Street { get; set; } = null!;
        public string Complement { get; set; } = null!;
        public string Neighborhood { get; set; } = null!;
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public int Number { get; set; }
        public Address() { }

        public Address(
            string cep,
            string street,
            string? complement,
            string neighborhood,
            string city,
            string state,
            int number)
        {
            if (string.IsNullOrWhiteSpace(cep)) throw new ArgumentException("CEP inválido", nameof(cep));
            if (string.IsNullOrWhiteSpace(street)) throw new ArgumentException("Logradouro inválido", nameof(street));
            if (string.IsNullOrWhiteSpace(neighborhood)) throw new ArgumentException("Bairro inválido", nameof(neighborhood));
            if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("Cidade inválida", nameof(city));
            if (string.IsNullOrWhiteSpace(state)) throw new ArgumentException("Estado inválido", nameof(state));

            Cep = cep;
            Street = street;
            Complement = complement ?? string.Empty;
            Neighborhood = neighborhood;
            City = city;
            State = state;
            Number = number;
        }
    }
}
