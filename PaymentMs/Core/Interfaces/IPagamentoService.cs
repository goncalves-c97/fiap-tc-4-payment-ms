using Core.Dtos;

namespace Core.Interfaces
{
    public interface IPagamentoService
    {
        Task<QrCodePagamentoDto> GetQrCodePagamento(PedidoPagamentoDto pedidoDto);
        Task<bool?> CheckPagamentoAprovado(string jsonNotificacao);
    }
}
