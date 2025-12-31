using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.Gateways;

namespace Core.Gateways
{
    public class NotificacaoWebhookGateway : INotificacaoWebhookGateway
    {
        private readonly IDbConnection _dbConnection;
        private const string _tableName = "Notificacao_webhook";

        public NotificacaoWebhookGateway(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task AddNew(string origem, int identificador, string dadoRecebido)
        {
            NotificacaoWebhook notificacao = new()
            {
                Origem = origem,
                Identificador = identificador,
                DadoRecebido = dadoRecebido,
                DataHora = DateTime.Now
            };

            var values = new Dictionary<string, object>
            {
                ["origem"] = notificacao.Origem,
                ["identificador"] = notificacao.Identificador,
                ["dado_recebido"] = notificacao.DadoRecebido,
                ["data_hora"] = notificacao.DataHora
            };

            await _dbConnection.InsertAndReturnIdAsync(_tableName, values, "id_notificacao_webhook");

            IEnumerable<NotificacaoWebhook> notificacoesInseridas = await GetByOrigemAndIdentificador(notificacao.Origem, notificacao.Identificador);

            if (!notificacoesInseridas.Any())
                throw new Exception("Insert failed");
        }

        public Task<IEnumerable<NotificacaoWebhook>> GetByOrigemAndIdentificador(string origem, int identificador)
        {
            return _dbConnection.SearchByParametersAsync<NotificacaoWebhook>(
                _tableName,
                "origem = @origem AND identificador = @identificador",
                new { origem, identificador }
            );
        }
    }
}
