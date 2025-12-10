using OrderManager.Application.Dto;
using OrderManager.Application.Interfaces;
using System.Net.Http.Json;

namespace OrderManager.Infrastructure.Integration
{
    public class FreightService : IFreightService
    {
        private readonly HttpClient _http;
        public FreightService(HttpClient http) => _http = http;

        public async Task<FreightQuoteResponseDto> GetFreightQuoteAsync(FreightQuoteRequestDto request, CancellationToken ct = default)
        {
            // Aqui vamos mockar: chamar um "mock endpoint"

            // exemplo: use remote mock service
            var res = await _http.PostAsJsonAsync("quote", request, ct);
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadFromJsonAsync<FreightQuoteResponseDto>(cancellationToken: ct)
                       ?? throw new InvalidOperationException("Resposta inválida do serviço de frete.");
            }

            // fallback local: cálculo simples
            var fallback = new FreightQuoteResponseDto
            {
                Price = Math.Round(10 + request.WeightKg * 2 + request.VolumeM3 * 50, 2),
                EstimatedDays = 5,
                Type = Domain.Enums.FreightType.Express
            };
            return fallback;
        }
    }
}
