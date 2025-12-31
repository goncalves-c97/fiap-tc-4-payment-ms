using Core.Dtos;
using Core.Interfaces.Gateways.Microservices;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Core.Gateways.Microservices
{
    public class OrderMsGateway : IOrderMsGateway
    {
        private readonly HttpClient _http;

        public OrderMsGateway(HttpClient http)
        {
            _http = http;
        }

        public async Task SetIdPagamentoOnPedido(int idPedido, int idPagamento, string token)
        {
            using var request = new HttpRequestMessage(
               HttpMethod.Put,
               $"Pedido/SetIdPagamento?idPedido={idPedido}&idPagamento={idPagamento}"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var response = await _http.SendAsync(request);

            string content = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
        }
    }
}
