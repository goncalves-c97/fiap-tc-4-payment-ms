namespace Core.Entities
{
    public class NotificacaoWebhook
    {
        public int IdNotificacaoWebhook { get; set; }
        public DateTime DataHora { get; set; }
        public string Origem { get; set; }
        public int Identificador { get; set; }
        public string DadoRecebido { get; set; }
    }
}
