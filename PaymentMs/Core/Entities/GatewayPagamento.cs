using System;
using System.Collections.Generic;

namespace Core.Entities;

public partial class GatewayPagamento
{
    public int IdGatewayPagamento { get; set; }

    public string Nome { get; set; } = null!;

    public virtual ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();
}
