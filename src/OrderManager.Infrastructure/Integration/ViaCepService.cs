using OrderManager.Application.Dto;
using OrderManager.Application.Interfaces;
using System.Net.Http.Json;

namespace OrderManager.Infrastructure.Integration
{
    public class ViaCepService : IViaCepService
    {
        private readonly HttpClient _http;
        public ViaCepService(HttpClient http) => _http = http;

        public async Task<ViaCepResponseDto?> GetAddressByCepAsync(string cep, CancellationToken ct = default)
        {
            // normalizar cep
            cep = new string(cep.Where(char.IsDigit).ToArray());
            var url = $"{cep}/json/";
            var res = await _http.GetAsync(url, ct);
            if (!res.IsSuccessStatusCode) return null;

            var dto = await res.Content.ReadFromJsonAsync<ViaCepResponseDto>(cancellationToken: ct);
            return dto;
        }
    }
}
