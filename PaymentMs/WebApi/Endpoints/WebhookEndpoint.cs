using Core.Controllers;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Endpoints
{
    [ApiController]
    [Route("Webhook/Pagamento")]
    public class WebhookEndpoint : ControllerBase
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPagamentoService _pagamentoService;

        public WebhookEndpoint(IDbConnection dbConnection, IPagamentoService pagamentoService)
        {
            _dbConnection = dbConnection;
            _pagamentoService = pagamentoService;
        }

        [HttpPost("")]
        public async Task<IActionResult> ReceberNotificacao(string origem, int identificador, [FromBody] object dadoRecebido)
        {
            await NotificacaoWebhookController.AddNewNotification(_dbConnection, _pagamentoService, origem, identificador, dadoRecebido);
            return Ok();
        }
    }

}
