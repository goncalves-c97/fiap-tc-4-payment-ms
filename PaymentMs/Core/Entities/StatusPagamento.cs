namespace Core.Entities;

public partial class StatusPagamento
{
    public int IdStatusPagamento { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();
}
