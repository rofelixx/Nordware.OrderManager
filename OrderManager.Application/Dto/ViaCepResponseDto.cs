namespace OrderManager.Application.Dto
{
    public record ViaCepResponseDto(
        string Cep,
        string Logradouro,
        string Complemento,
        string Bairro,
        string Localidade,
        string Uf
    );
}
