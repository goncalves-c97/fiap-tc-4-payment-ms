using Core.Dtos;

namespace Core.Interfaces.Gateways.Microservices
{
    public interface IOrderMsGateway
    {
        Task SetIdPagamentoOnPedido(int idPedido, int idPagamento, string token);
    }
}
