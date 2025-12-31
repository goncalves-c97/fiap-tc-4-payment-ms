using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.Gateways;

namespace Core.Gateways
{
    public class UrlWebhookGateway : IUrlWebhookGateway
    {
        private readonly IDbConnection _dbConnection;
        private const string _tableName = "Url_webhook";

        public UrlWebhookGateway(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<string> GetUrl()
        {
            UrlWebhook? urlWebhook = await _dbConnection.SearchFirstOrDefaultByParametersAsync<UrlWebhook>(_tableName, "1 = 1");

            if (urlWebhook == null)
                return "https://www.google.com.br";
            else
                return urlWebhook.Url;
        }

        public async Task InsertUpdateUrl(string url)
        {
            if(string.IsNullOrEmpty(url)) 
                return;

            await _dbConnection.DeleteAsync(_tableName, "1 = 1");
            await _dbConnection.InsertAsync(_tableName, new Dictionary<string, object> { ["url"] = url });
        }
    }
}
