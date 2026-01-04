using Core.Constants;
using Core.Controllers;
using Core.Dtos;
using Core.Entities;
using Core.Enums;
using Core.Gateways.Microservices;
using Core.Interfaces;
using Core.Interfaces.Gateways.Microservices;
using Core.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace WebApi.Endpoints
{
    [ApiController]
    [Route("Pedido")]
    public class PedidoEndpoint(IDbConnection dbConnection, IPagamentoService pagamentoService, IOrderMsGateway orderMsGateway, IOptions<AppSettings> appSettings) : ControllerBase
    {
        private readonly IDbConnection _dbConnection = dbConnection;
        private readonly IPagamentoService _pagamentoService = pagamentoService;
        private readonly IOrderMsGateway _orderMsGateway = orderMsGateway;
        private readonly IOptions<AppSettings> _appSettings = appSettings;

        [Authorize(Roles = $"{UsuarioRoles.Administrador}, {UsuarioRoles.Cozinheiro}")]
        [HttpGet, Route("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] StatusPagamentoEnum? status)
        {
            return Ok(await PagamentoController.GetAllPagamentos(_dbConnection, status));
        }

        [Authorize]
        [HttpGet, Route("GetById")]
        public async Task<IActionResult> GetById([FromQuery] int idPedido)
        {
            return Ok(await PagamentoController.GetPedidoById(_dbConnection, idPedido));
        }

        [Authorize(Roles = $"{UsuarioRoles.ClienteIdentificado}, {UsuarioRoles.ClienteAnonimo}")]
        [HttpPost, Route("CheckoutPedido")]
        public async Task<IActionResult> ConfirmaPedido([FromQuery] int idPedido, int valorPedido)
        {
            QrCodePagamentoDto qrCodePagamento = await PagamentoController.CheckoutPedido(_dbConnection, _appSettings, _pagamentoService, _orderMsGateway, idPedido, valorPedido, GetRequestToken(this));
            return Ok(qrCodePagamento);
        }

        [Authorize(Roles = $"{UsuarioRoles.ClienteIdentificado}, {UsuarioRoles.ClienteAnonimo}")]
        [HttpGet, Route("CheckStatusPagamentoPedido")]
        public async Task<IActionResult> CheckStatusPagamentoPedido([FromQuery] int idPedido)
        {
            StatusPagamentoEnum statusPagamentoEnum = await PagamentoController.CheckStatusPagamentoPedido(_dbConnection, idPedido);
            return Ok(statusPagamentoEnum.ToString());
        }

        [Authorize(Roles = $"{UsuarioRoles.ClienteIdentificado}, {UsuarioRoles.ClienteAnonimo}")]
        [HttpPut, Route("InformaPagamentoPedidoAprovado")]
        public async Task<IActionResult> InformaPagamentoPedidoAprovado([FromQuery] int idPedido)
        {
            await PagamentoController.InformaPagamentoPedidoAprovado(_dbConnection, _appSettings, idPedido);
            return Ok();
        }

        [Authorize(Roles = $"{UsuarioRoles.ClienteIdentificado}, {UsuarioRoles.ClienteAnonimo}")]
        [HttpPut, Route("InformaPagamentoPedidoNegado")]
        public async Task<IActionResult> InformaPagamentoPedidoNegado([FromQuery] int idPedido)
        {
            await PagamentoController.InformaPagamentoPedidoNegado(_dbConnection, idPedido);
            return Ok();
        }

        [NonAction]
        private static string GetRequestToken(ControllerBase context)
        {
            return context.Request.Headers.Authorization
                .ToString()
                .Replace("Bearer ", string.Empty);
        }
    }
}
