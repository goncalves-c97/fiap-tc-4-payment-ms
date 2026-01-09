using Core.Dtos;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Gateways;
using Core.Interfaces.Gateways.Microservices;
using Core.Settings;
using Core.UseCases;
using Microsoft.Extensions.Options;
using Moq;

namespace Test.UseCases;

public class PagamentoUseCasesTests
{
    [Fact]
    public async Task GetAllPagamentos_WhenGatewayIsNull_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
        PagamentoUseCases.GetAllPagamentos(null!));
    }

    [Fact]
    public async Task GetAllPagamentos_WhenCalled_DelegatesToGateway()
    {
        var expected = new List<Pagamento>
 {
 new() { IdPagamento =1, IdPedido =10, IdGatewayPagamento =1, Valor =12.5m, IdStatusPagamento = (int)StatusPagamentoEnum.Pendente }
 };

        var gateway = new Mock<IPagamentoGateway>(MockBehavior.Strict);
        gateway.Setup(g => g.GetAllPagamentos(null)).ReturnsAsync(expected);

        var result = await PagamentoUseCases.GetAllPagamentos(gateway.Object, null);

        Assert.Same(expected, result);
        gateway.VerifyAll();
    }

    [Fact]
    public async Task GetQrCodePagamento_WhenServiceIsNull_ThrowsArgumentNullException()
    {
        var dto = new PedidoPagamentoDto { PedidoId = "1" };

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
        PagamentoUseCases.GetQrCodePagamento(null!, dto));
    }

    [Fact]
    public async Task GetQrCodePagamento_WhenDtoIsNull_ThrowsArgumentNullException()
    {
        var service = new Mock<IPagamentoService>(MockBehavior.Strict);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
        PagamentoUseCases.GetQrCodePagamento(service.Object, null!));
    }

    [Fact]
    public async Task CreatePagamento_CreatesPendenteMercadoPagoPagamento_WithExpectedValues()
    {
        Pagamento? captured = null;

        var gateway = new Mock<IPagamentoGateway>(MockBehavior.Strict);
        gateway
        .Setup(g => g.InsertPagamento(It.IsAny<Pagamento>()))
        .Callback<Pagamento>(p => captured = p)
        .ReturnsAsync((Pagamento p) => p);

        var result = await PagamentoUseCases.CreatePagamento(gateway.Object, idPedido: 123, valor: 9.99m);

        Assert.NotNull(captured);
        Assert.Equal(123, captured!.IdPedido);
        Assert.Equal((int)GatewayPagamentoEnum.MercadoPago, captured.IdGatewayPagamento);
        Assert.Equal(9.99m, captured.Valor);
        Assert.Equal((int)StatusPagamentoEnum.Pendente, captured.IdStatusPagamento);
        Assert.Null(captured.DataHoraPago);

        Assert.Same(captured, result);
        gateway.VerifyAll();
    }

    [Fact]
    public async Task GetPagamentoByIdPedido_WhenGatewayIsNull_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
        PagamentoUseCases.GetPagamentoByIdPedido(null!, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetPagamentoByIdPedido_WhenIdPedidoInvalid_ThrowsArgumentOutOfRangeException(int idPedido)
    {
        var gateway = new Mock<IPagamentoGateway>(MockBehavior.Strict);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
        PagamentoUseCases.GetPagamentoByIdPedido(gateway.Object, idPedido));
    }

    [Fact]
    public async Task GetPagamentoByIdPedido_WhenNotFound_ThrowsKeyNotFoundException()
    {
        var gateway = new Mock<IPagamentoGateway>(MockBehavior.Strict);
        gateway.Setup(g => g.GetByPedidoId(10)).ReturnsAsync((Pagamento?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
        PagamentoUseCases.GetPagamentoByIdPedido(gateway.Object, 10));

        gateway.VerifyAll();
    }

    [Fact]
    public async Task GetPagamentoByIdPedido_WhenFound_ReturnsPagamento()
    {
        var expected = new Pagamento { IdPagamento = 55, IdPedido = 10, IdStatusPagamento = (int)StatusPagamentoEnum.Pendente };

        var gateway = new Mock<IPagamentoGateway>(MockBehavior.Strict);
        gateway.Setup(g => g.GetByPedidoId(10)).ReturnsAsync(expected);

        var result = await PagamentoUseCases.GetPagamentoByIdPedido(gateway.Object, 10);

        Assert.Same(expected, result);
        gateway.VerifyAll();
    }

    [Fact]
    public async Task VerificaNotificacaoPagamento_WhenServiceReturnsNull_DoesNotUpdateStatus()
    {
        var gateway = new Mock<IPagamentoGateway>(MockBehavior.Strict);
        var service = new Mock<IPagamentoService>(MockBehavior.Strict);

        service.Setup(s => s.CheckPagamentoAprovado("payload")).ReturnsAsync((bool?)null);

        await PagamentoUseCases.VerificaNotificacaoPagamento(gateway.Object, service.Object, idPedido: 1, jsonNotificacao: "payload");

        gateway.VerifyNoOtherCalls();
        service.VerifyAll();
    }

    [Fact]
    public async Task VerificaNotificacaoPagamento_WhenApproved_UpdatesPagamentoToAprovado()
    {
        var pagamento = new Pagamento { IdPagamento = 99, IdPedido = 1, IdStatusPagamento = (int)StatusPagamentoEnum.Pendente };

        var gateway = new Mock<IPagamentoGateway>(MockBehavior.Strict);
        var service = new Mock<IPagamentoService>(MockBehavior.Strict);

        service.Setup(s => s.CheckPagamentoAprovado("payload")).ReturnsAsync(true);
        gateway.Setup(g => g.GetByPedidoId(1)).ReturnsAsync(pagamento);
        gateway.Setup(g => g.UpdatePagamento(It.Is<Pagamento>(p =>
        p.IdPedido == 1 && p.IdStatusPagamento == (int)StatusPagamentoEnum.Aprovado && p.DataHoraPago != null)))
        .Returns(Task.CompletedTask);

        await PagamentoUseCases.VerificaNotificacaoPagamento(gateway.Object, service.Object, idPedido: 1, jsonNotificacao: "payload");

        gateway.VerifyAll();
        service.VerifyAll();
    }

    [Fact]
    public async Task VerificaNotificacaoPagamento_WhenDenied_UpdatesPagamentoToNegado_WithoutPagoDate()
    {
        var pagamento = new Pagamento { IdPagamento = 99, IdPedido = 1, IdStatusPagamento = (int)StatusPagamentoEnum.Pendente, DataHoraPago = null };

        var gateway = new Mock<IPagamentoGateway>(MockBehavior.Strict);
        var service = new Mock<IPagamentoService>(MockBehavior.Strict);

        service.Setup(s => s.CheckPagamentoAprovado("payload")).ReturnsAsync(false);
        gateway.Setup(g => g.GetByPedidoId(1)).ReturnsAsync(pagamento);
        gateway.Setup(g => g.UpdatePagamento(It.Is<Pagamento>(p =>
        p.IdPedido == 1 && p.IdStatusPagamento == (int)StatusPagamentoEnum.Negado && p.DataHoraPago == null)))
        .Returns(Task.CompletedTask);

        await PagamentoUseCases.VerificaNotificacaoPagamento(gateway.Object, service.Object, idPedido: 1, jsonNotificacao: "payload");

        gateway.VerifyAll();
        service.VerifyAll();
    }

    [Fact]
    public async Task CreatePagamentoAndGetQrCodePagamento_CallsDependencies_AndReturnsQrCode()
    {
        var appSettings = Options.Create(new AppSettings { AppUrl = "http://localhost" });

        var pagamentoGateway = new Mock<IPagamentoGateway>(MockBehavior.Strict);
        var pagamentoService = new Mock<IPagamentoService>(MockBehavior.Strict);
        var orderGateway = new Mock<IOrderMsGateway>(MockBehavior.Strict);

        var qrExpected = new QrCodePagamentoDto("123", "MercadoPago", "qr");

        pagamentoService
        .Setup(s => s.GetQrCodePagamento(It.IsAny<PedidoPagamentoDto>()))
        .ReturnsAsync(qrExpected);

        pagamentoGateway
        .Setup(g => g.InsertPagamento(It.IsAny<Pagamento>()))
        .ReturnsAsync((Pagamento p) => { p.IdPagamento = 777; return p; });

        orderGateway
        .Setup(g => g.SetIdPagamentoOnPedido(123, 777, "token"))
        .Returns(Task.CompletedTask);

        var result = await PagamentoUseCases.CreatePagamentoAndGetQrCodePagamento(
        appSettings,
        pagamentoGateway.Object,
        pagamentoService.Object,
        orderGateway.Object,
        idPedido: 123,
        valorPedido: 1999,
        token: "token");

        Assert.Same(qrExpected, result);

        pagamentoService.Verify(s => s.GetQrCodePagamento(It.Is<PedidoPagamentoDto>(p =>
        p.PedidoId == "123" &&
        p.TituloPedido == "PEDIDO #123" &&
        p.ValorTotal == 19.99m &&
        p.UrlNotificacao.Contains("/Webhook/Pagamento", StringComparison.OrdinalIgnoreCase)
       )), Times.Once);

        pagamentoGateway.Verify(g => g.InsertPagamento(It.IsAny<Pagamento>()), Times.Once);
        orderGateway.VerifyAll();
    }
}
