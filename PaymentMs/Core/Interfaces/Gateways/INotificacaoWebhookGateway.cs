using Core.Entities;

namespace Core.Interfaces.Gateways
{
    public interface INotificacaoWebhookGateway
    {
        Task AddNew(string origem, int identificador, string dadoRecebido);
        Task<IEnumerable<NotificacaoWebhook>> GetByOrigemAndIdentificador(string origem, int identificador);
    }
}
