using Core.Adapters;
using Core.Gateways;
using Core.Interfaces;
using Core.UseCases;

namespace Core.Controllers
{
    public static class NotificacaoWebhookController
    {
        public static async Task SetWebhookBaseUrl(IDbConnection dbConnection, string url)
        {
            UrlWebhookGateway gateway = new(dbConnection);
            await gateway.InsertUpdateUrl(url);
        }

        public static async Task AddNewNotification(IDbConnection dbConnection, IPagamentoService pagamentoService, string origem, int identificador, object dadoRecebido)
        {
            NotificacaoWebhookGateway gateway = new(dbConnection);
            string dadoRecebidoJson = ObjectToJsonStringAdapter.ConvertToJsonString(dadoRecebido);
            await gateway.AddNew(origem, identificador, dadoRecebidoJson);

            PagamentoGateway pagamentoGateway = new(dbConnection);

            await PagamentoUseCases.VerificaNotificacaoPagamento(pagamentoGateway, pagamentoService, identificador, dadoRecebidoJson);
        }
    }
}
