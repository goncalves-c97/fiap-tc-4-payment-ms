using Core.Dtos;
using Core.Entities;

namespace Test.Models;

public class DtoAndEntityTests
{
    [Fact]
    public void PedidoPagamentoDto_Defaults_CanSetAndGet()
    {
        var dto = new PedidoPagamentoDto
        {
            PedidoId = "1",
            TituloPedido = "t",
            Description = "d",
            UrlNotificacao = "u",
            DataHoraExpiracaoPagamento = new DateTime(2025, 1, 1),
            ValorTotal = 10.5m
        };

        Assert.Equal("1", dto.PedidoId);
        Assert.Equal("t", dto.TituloPedido);
        Assert.Equal("d", dto.Description);
        Assert.Equal("u", dto.UrlNotificacao);
        Assert.Equal(new DateTime(2025, 1, 1), dto.DataHoraExpiracaoPagamento);
        Assert.Equal(10.5m, dto.ValorTotal);
    }

    [Fact]
    public void QrCodePagamentoDto_Ctor_SetsProperties()
    {
        var dto = new QrCodePagamentoDto("p", "prov", "qr");

        Assert.Equal("p", dto.PedidoId);
        Assert.Equal("prov", dto.ProvedorId);
        Assert.Equal("qr", dto.QrCodeValue);
    }

    [Fact]
    public void Pagamento_Properties_CanSetAndGet()
    {
        var p = new Pagamento
        {
            IdPagamento = 1,
            IdPedido = 2,
            IdGatewayPagamento = 3,
            Valor = 4.5m,
            IdStatusPagamento = 1,
            DataHoraPago = null
        };

        Assert.Equal(1, p.IdPagamento);
        Assert.Equal(2, p.IdPedido);
        Assert.Equal(3, p.IdGatewayPagamento);
        Assert.Equal(4.5m, p.Valor);
        Assert.Equal(1, p.IdStatusPagamento);
        Assert.Null(p.DataHoraPago);
    }
}
