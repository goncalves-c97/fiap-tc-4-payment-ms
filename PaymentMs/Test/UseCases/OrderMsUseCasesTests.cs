using Core.Interfaces.Gateways.Microservices;
using Core.UseCases.Microservices;
using Moq;

namespace Test.UseCases;

public class OrderMsUseCasesTests
{
    [Fact]
    public async Task VinculaPagamentoPendenteAoPedido_CallsGateway()
    {
        var gateway = new Mock<IOrderMsGateway>(MockBehavior.Strict);
        gateway.Setup(g => g.SetIdPagamentoOnPedido(1, 2, "t")).Returns(Task.CompletedTask);

        await OrderMsUseCases.VinculaPagamentoPendenteAoPedido(gateway.Object, 1, 2, "t");

        gateway.VerifyAll();
    }
}
