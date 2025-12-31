namespace Core.Interfaces.Gateways
{
    public interface IUrlWebhookGateway
    {
        Task InsertUpdateUrl(string url);
        Task<string> GetUrl();
    }
}
