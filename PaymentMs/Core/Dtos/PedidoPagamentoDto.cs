namespace Core.Dtos
{
    public class PedidoPagamentoDto
    {
        public string PedidoId { get; set; }
        public string TituloPedido { get; set; }
        public string Description { get; set; }
        public string UrlNotificacao { get; set; }
        public DateTime DataHoraExpiracaoPagamento { get; set; }
        public decimal ValorTotal { get; set; }

    }
}
