using Core.Dtos;
using Core.Entities;
using Core.Enums;
using Core.Gateways;
using Core.Interfaces;
using Core.Interfaces.Gateways;
using Core.Interfaces.Gateways.Microservices;
using Core.Settings;
using Core.UseCases.Microservices;
using Microsoft.Extensions.Options;

namespace Core.UseCases
{
    public static class PagamentoUseCases
    {
        public static async Task<IEnumerable<Pagamento>> GetAllPagamentos(IPagamentoGateway pagamentoGateway, StatusPagamentoEnum? status = null)
        {
            if (pagamentoGateway == null)
                throw new ArgumentNullException(nameof(pagamentoGateway), "O gateway de pagamento não pode ser nulo.");
            
            return await pagamentoGateway.GetAllPagamentos(status);
        }
        public static async Task<QrCodePagamentoDto> GetQrCodePagamento(IPagamentoService pagamentoService, PedidoPagamentoDto pedidoDto)
        {
            if (pagamentoService == null)
                throw new ArgumentNullException(nameof(pagamentoService), "O serviço de pagamento não pode ser nulo.");

            if (pedidoDto == null)
                throw new ArgumentNullException(nameof(pedidoDto), "O pedido de pagamento não pode ser nulo.");

            return await pagamentoService.GetQrCodePagamento(pedidoDto);
        }

        public static async Task<QrCodePagamentoDto> CreatePagamentoAndGetQrCodePagamento(IOptions<AppSettings> appSettings, IPagamentoGateway pagamentoGateway, IPagamentoService pagamentoService, IOrderMsGateway orderMsGateway, int idPedido, int valorPedido, string token)
        {
            // Criação do pedido para o serviço de pagamento
            PedidoPagamentoDto pedidoPagamentoDto = new()
            {
                PedidoId = idPedido.ToString(),
                TituloPedido = $"PEDIDO #{idPedido}",
                Description = "Pedido FastFoodChallenge",
                UrlNotificacao = $"https://www.google.com.br/Webhook/Pagamento?origem=MercadoPago&identificador={idPedido}", //TODO: Fix route {appSettings.Value.AppUrl}
                DataHoraExpiracaoPagamento = DateTime.Now.AddMinutes(10),
                ValorTotal = valorPedido / 100m // Converte centavos para reais
            };

            // Obtém o QRCode para o pagamento
            QrCodePagamentoDto qrCodePagamento = await GetQrCodePagamento(pagamentoService, pedidoPagamentoDto);

            // Cria o registro de pagamento
            Pagamento pagamento = await CreatePagamento(pagamentoGateway, idPedido, pedidoPagamentoDto.ValorTotal);

            // Vincula o pagamento pendente ao registro do pedido
            await OrderMsUseCases.VinculaPagamentoPendenteAoPedido(orderMsGateway, idPedido, pagamento.IdPagamento, token);

            // Retorna o QRCode para pagamento
            return qrCodePagamento;
        }

        public static async Task<Pagamento> CreatePagamento(IPagamentoGateway pagamentoGateway, int idPedido, decimal valor)
        {
            Pagamento pagamento = new()
            {
                IdPedido = idPedido,
                IdGatewayPagamento = (int)GatewayPagamentoEnum.MercadoPago,
                Valor = valor,
                IdStatusPagamento = (int)StatusPagamentoEnum.Pendente,
                DataHoraPago = null
            };

            return await pagamentoGateway.InsertPagamento(pagamento);
        }

        private static async Task UpdateStatusPagamento(IPagamentoGateway pagamentoGateway, int idPedido, StatusPagamentoEnum statusPagamento)
        {
            if (pagamentoGateway == null)
                throw new ArgumentNullException(nameof(pagamentoGateway), "O gateway de pagamento não pode ser nulo.");

            if (idPedido <= 0)
                throw new ArgumentOutOfRangeException(nameof(idPedido), "O ID de pedido deve ser maior que zero.");

            Pagamento pagamento = await pagamentoGateway.GetByPedidoId(idPedido)
                ?? throw new KeyNotFoundException($"Pagamento com ID de pedido {idPedido} não encontrado.");

            pagamento.IdStatusPagamento = (int)statusPagamento;

            if (statusPagamento == StatusPagamentoEnum.Aprovado)
                pagamento.DataHoraPago = DateTime.Now;

            await pagamentoGateway.UpdatePagamento(pagamento);
        }

        public static async Task SetPagamentoPedidoAprovado(IPagamentoGateway pagamentoGateway, int idPedido)
        {
            await UpdateStatusPagamento(pagamentoGateway, idPedido, StatusPagamentoEnum.Aprovado);
        }

        public static async Task SetPagamentoPedidoNegado(IPagamentoGateway pagamentoGateway, int idPedido)
        {
            await UpdateStatusPagamento(pagamentoGateway, idPedido, StatusPagamentoEnum.Negado);
        }

        public static async Task SetPagamentoAprovado(IPagamentoGateway pagamentoGateway, IOptions<AppSettings> appSettings, int idPagamento)
        {
            await UpdateStatusPagamento(pagamentoGateway, idPagamento, StatusPagamentoEnum.Aprovado);

            // TODO: ????
            //await new HttpClient().PutAsync(
            //     $"{appSettings.Value.OrderMicroserviceUrl}/InformaPagamentoPedidoAprovado",
            //     null);
        }

        public static async Task SetPagamentoNegado(IPagamentoGateway pagamentoGateway, int idPagamento)
        {
            await UpdateStatusPagamento(pagamentoGateway, idPagamento, StatusPagamentoEnum.Negado);
        }

        public static async Task<StatusPagamentoEnum> VerificaStatusPagamentoPedido(PagamentoGateway pagamentoGateway, int idPedido)
        {
            Pagamento? pagamento = await pagamentoGateway.GetByPedidoId(idPedido)
                ?? throw new KeyNotFoundException($"Pagamento com ID de pedido {idPedido} não encontrado.");

            return (StatusPagamentoEnum)pagamento.IdStatusPagamento;
        }

        public static async Task VerificaNotificacaoPagamento(IPagamentoGateway pagamentoGateway, IPagamentoService pagamentoService, int idPedido, string jsonNotificacao)
        {
            bool? resultadoPagamento = await pagamentoService.CheckPagamentoAprovado(jsonNotificacao);

            if (resultadoPagamento == null)
                return;

            if ((bool)resultadoPagamento)
                await SetPagamentoPedidoAprovado(pagamentoGateway, idPedido);
            else
                await SetPagamentoPedidoNegado(pagamentoGateway, idPedido);
        }
        public static async Task<Pagamento> GetPagamentoByIdPedido(IPagamentoGateway pagamentoGateway, int idPedido)
        {
            if (pagamentoGateway == null)
                throw new ArgumentNullException(nameof(pagamentoGateway), "O gateway de pagamento não pode ser nulo.");
            if (idPedido <= 0)
                throw new ArgumentOutOfRangeException(nameof(idPedido), "O ID de pedido deve ser maior que zero.");
            Pagamento? pagamento = await pagamentoGateway.GetByPedidoId(idPedido)
                ?? throw new KeyNotFoundException($"Pagamento com ID de pedido {idPedido} não encontrado.");

            return pagamento;
        }        
    }
}