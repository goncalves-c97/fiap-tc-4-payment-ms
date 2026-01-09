using Core.Dtos;
using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.Gateways.Microservices;
using Core.Settings;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;

namespace Test.WebApi;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            // Provide required configuration keys so Program.cs does not throw.
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["API_AUTHENTICATION_KEY"] = "super_secret_test_key_1234567890123456",
                ["DB_CONNECTION_STRING"] = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;",
                ["DB_NAME"] = "PaymentMs_Test",
                ["ORDER_MS_URL"] = "http://localhost/",
                ["APP_SETTINGS:AppUrl"] = "http://localhost"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace real implementations with mocks to avoid hitting external services/databases.
            var db = new Mock<IDbConnection>(MockBehavior.Loose);

            // Used by PagamentoGateway.GetAllPagamentos when status == null
            db.Setup(d => d.ListAllAsync<Pagamento>(It.IsAny<string>(), It.IsAny<string[]?>()))
     .ReturnsAsync([]);

            // Used by PagamentoGateway.GetByPedidoId
            db.Setup(d => d.SearchFirstOrDefaultByParametersAsync<Pagamento>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()))
     .ReturnsAsync(new Pagamento { IdPagamento = 1, IdPedido = 1, IdStatusPagamento = 1, IdGatewayPagamento = 1, Valor = 1m, DataHoraPago = null });

            // Used by PagamentoGateway.UpdatePagamento
            db.Setup(d => d.UpdateAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), It.IsAny<object?>()))
     .ReturnsAsync(1);

            // Used by PagamentoGateway.InsertPagamento
            db.Setup(d => d.InsertAndReturnIdAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
     .ReturnsAsync(1);

            services.AddSingleton(db.Object);

            var pagamentoService = new Mock<IPagamentoService>(MockBehavior.Loose);
            pagamentoService
     .Setup(s => s.GetQrCodePagamento(It.IsAny<PedidoPagamentoDto>()))
     .ReturnsAsync(new QrCodePagamentoDto("1", "prov", "qr"));
            pagamentoService
     .Setup(s => s.CheckPagamentoAprovado(It.IsAny<string>()))
     .ReturnsAsync((bool?)true);
            services.AddSingleton(pagamentoService.Object);

            var orderGateway = new Mock<IOrderMsGateway>(MockBehavior.Loose);
            orderGateway.Setup(g => g.SetIdPagamentoOnPedido(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
     .Returns(Task.CompletedTask);
            services.AddSingleton(orderGateway.Object);

            // Ensure AppSettings exists.
            services.AddSingleton<IOptions<AppSettings>>(Options.Create(new AppSettings { AppUrl = "http://localhost" }));
        });

        return base.CreateHost(builder);
    }
}
