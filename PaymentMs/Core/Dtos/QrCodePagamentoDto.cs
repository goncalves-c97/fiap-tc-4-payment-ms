namespace Core.Dtos
{
    public class QrCodePagamentoDto
    {
        public string PedidoId { get; set; }
        public string ProvedorId { get; set; }
        public string QrCodeValue { get; set; }

        public QrCodePagamentoDto(string pedidoId, string provedorId, string qrCodeValue)
        {
            PedidoId = pedidoId;
            ProvedorId = provedorId;
            QrCodeValue = qrCodeValue;
        }
    }
}
