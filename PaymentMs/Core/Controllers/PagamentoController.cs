using Core.Dtos;
using Core.Entities;
using Core.Enums;
using Core.Gateways;
using Core.Interfaces;
using Core.Interfaces.Gateways.Microservices;
using Core.Settings;
using Core.UseCases;
using Microsoft.Extensions.Options;

namespace Core.Controllers
{
    public static class PagamentoController
    {
        public static async Task<IEnumerable<Pagamento>> GetAllPagamentos(IDbConnection dbConnection, StatusPagamentoEnum? status = null)
        {
            PagamentoGateway gateway = new(dbConnection);
            IEnumerable<Pagamento> pagamentos = await PagamentoUseCases.GetAllPagamentos(gateway, status);
            return pagamentos;
        }

        public static async Task<Pagamento> GetPedidoById(IDbConnection dbConnection, int idPedido)
        {
            PagamentoGateway gateway = new(dbConnection);
            Pagamento pagamento = await PagamentoUseCases.GetPagamentoByIdPedido(gateway, idPedido);
            return pagamento;
        }

        public static async Task<QrCodePagamentoDto> CheckoutPedido(IDbConnection dbConnection, IOptions<AppSettings> appSettings, IPagamentoService pagamentoService, IOrderMsGateway orderMsGateway, int idPedido, int valorPedido, string token)
        {
            PagamentoGateway pagamentoGateway = new(dbConnection);

            return await PagamentoUseCases.CreatePagamentoAndGetQrCodePagamento(appSettings, pagamentoGateway, pagamentoService, orderMsGateway, idPedido, valorPedido, token);
        }

        public static async Task InformaPagamentoPedidoAprovado(IDbConnection dbConnection, IOptions<AppSettings> appSettings, int idPedido)
        {
            PagamentoGateway pagamentoGateway = new(dbConnection);

            await PagamentoUseCases.SetPagamentoAprovado(pagamentoGateway, appSettings, idPedido);
        }

        public static async Task InformaPagamentoPedidoNegado(IDbConnection dbConnection, int idPedido)
        {
            PagamentoGateway pagamentoGateway = new(dbConnection);

            await PagamentoUseCases.SetPagamentoNegado(pagamentoGateway, idPedido);
        }

        public static async Task<StatusPagamentoEnum> CheckStatusPagamentoPedido(IDbConnection dbConnection, int idPedido)
        {
            PagamentoGateway pagamentoGateway = new(dbConnection);
            StatusPagamentoEnum statusPagamento = await PagamentoUseCases.VerificaStatusPagamentoPedido(pagamentoGateway, idPedido);
            return statusPagamento;
        }
    }
}
