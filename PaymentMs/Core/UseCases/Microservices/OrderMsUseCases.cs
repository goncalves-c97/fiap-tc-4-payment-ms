using Core.Interfaces.Gateways.Microservices;

namespace Core.UseCases.Microservices
{
    public static class OrderMsUseCases
    {
        public static async Task VinculaPagamentoPendenteAoPedido(IOrderMsGateway orderMsGateway, int idPedido, int idPagamento, string token)
        {
            await orderMsGateway.SetIdPagamentoOnPedido(idPedido, idPagamento, token);
        }
    }
}
