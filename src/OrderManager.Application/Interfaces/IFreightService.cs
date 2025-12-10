using OrderManager.Application.Dto;

namespace OrderManager.Application.Interfaces
{
    public interface IFreightService
    {
        Task<FreightQuoteResponseDto> GetFreightQuoteAsync(FreightQuoteRequestDto request, CancellationToken ct = default);
    }
}
