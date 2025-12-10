using OrderManager.Application.Dto;

namespace OrderManager.Application.Interfaces
{
    public interface IViaCepService
    {
        Task<ViaCepResponseDto?> GetAddressByCepAsync(string cep, CancellationToken ct = default);
    }
}
