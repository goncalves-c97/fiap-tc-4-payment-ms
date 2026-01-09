using Core.Entities;
using Core.Enums;
using Core.Interfaces.Gateways;
using Core.UseCases;
using Moq;

namespace Test.UseCases;

public class PagamentoUseCases_UpdateStatusTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SetPagamentoPedidoAprovado_WhenIdPedidoInvalid_Throws(int idPedido)
    {
        var gateway = new Mock<IPagamentoGateway>(MockBehavior.Strict);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
        PagamentoUseCases.SetPagamentoPedidoAprovado(gateway.Object, idPedido));
    }

    [Fact]
    public async Task SetPagamentoPedidoAprovado_WhenPagamentoMissing_ThrowsKeyNotFound()
    {
        var gateway = new Mock<IPagamentoGateway>(MockBehavior.Strict);
        gateway.Setup(g => g.GetByPedidoId(10)).ReturnsAsync((Pagamento?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
        PagamentoUseCases.SetPagamentoPedidoAprovado(gateway.Object, 10));

        gateway.VerifyAll();
    }

    [Fact]
    public async Task SetPagamentoPedidoAprovado_WhenFound_SetsStatusAndPagoDate()
    {
        var pagamento = new Pagamento { IdPagamento = 1, IdPedido = 10, IdStatusPagamento = (int)StatusPagamentoEnum.Pendente, DataHoraPago = null };

        var gateway = new Mock<IPagamentoGateway>(MockBehavior.Strict);
        gateway.Setup(g => g.GetByPedidoId(10)).ReturnsAsync(pagamento);
        gateway.Setup(g => g.UpdatePagamento(It.Is<Pagamento>(p =>
        p.IdPedido == 10 && p.IdStatusPagamento == (int)StatusPagamentoEnum.Aprovado && p.DataHoraPago != null)))
        .Returns(Task.CompletedTask);

        await PagamentoUseCases.SetPagamentoPedidoAprovado(gateway.Object, 10);

        gateway.VerifyAll();
    }

    [Fact]
    public async Task SetPagamentoPedidoNegado_WhenFound_SetsStatusAndKeepsPagoDateNull()
    {
        var pagamento = new Pagamento { IdPagamento = 1, IdPedido = 10, IdStatusPagamento = (int)StatusPagamentoEnum.Pendente, DataHoraPago = null };

        var gateway = new Mock<IPagamentoGateway>(MockBehavior.Strict);
        gateway.Setup(g => g.GetByPedidoId(10)).ReturnsAsync(pagamento);
        gateway.Setup(g => g.UpdatePagamento(It.Is<Pagamento>(p =>
        p.IdPedido == 10 && p.IdStatusPagamento == (int)StatusPagamentoEnum.Negado && p.DataHoraPago == null)))
        .Returns(Task.CompletedTask);

        await PagamentoUseCases.SetPagamentoPedidoNegado(gateway.Object, 10);

        gateway.VerifyAll();
    }
}
