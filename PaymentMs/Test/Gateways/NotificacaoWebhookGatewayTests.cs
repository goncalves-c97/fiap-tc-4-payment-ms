using Core.Gateways;
using Moq;

namespace Test.Gateways;

public class NotificacaoWebhookGatewayTests
{
    [Fact]
    public async Task AddNew_WhenAfterInsertSearchReturnsEmpty_Throws()
    {
        var db = new Mock<Core.Interfaces.IDbConnection>(MockBehavior.Strict);
        db.Setup(d => d.InsertAndReturnIdAsync(
        "Notificacao_webhook",
        It.IsAny<Dictionary<string, object>>(),
        "id_notificacao_webhook")).ReturnsAsync(1);

        db.Setup(d => d.SearchByParametersAsync<Core.Entities.NotificacaoWebhook>(
        "Notificacao_webhook",
        It.IsAny<string>(),
        It.IsAny<object>())).ReturnsAsync([]);

        var gateway = new NotificacaoWebhookGateway(db.Object);

        await Assert.ThrowsAsync<Exception>(() => gateway.AddNew("origem", 10, "{}"));
    }

    [Fact]
    public async Task AddNew_WhenSearchReturnsAny_DoesNotThrow()
    {
        var db = new Mock<Core.Interfaces.IDbConnection>(MockBehavior.Strict);
        db.Setup(d => d.InsertAndReturnIdAsync(
        "Notificacao_webhook",
        It.IsAny<Dictionary<string, object>>(),
        "id_notificacao_webhook")).ReturnsAsync(1);

        db.Setup(d => d.SearchByParametersAsync<Core.Entities.NotificacaoWebhook>(
        "Notificacao_webhook",
        "origem = @origem AND identificador = @identificador",
        It.IsAny<object>())).ReturnsAsync([new Core.Entities.NotificacaoWebhook()]);

        var gateway = new NotificacaoWebhookGateway(db.Object);

        await gateway.AddNew("origem", 10, "{}");

        db.VerifyAll();
    }
}
