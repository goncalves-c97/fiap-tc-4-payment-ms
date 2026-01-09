using Core.Constants;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Test.Helpers;

namespace Test.WebApi;

public class SmokeTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SmokeTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;

        Environment.SetEnvironmentVariable(
        "API_AUTHENTICATION_KEY",
        "0123456789ABCDEF0123456789ABCDEF");

        Environment.SetEnvironmentVariable("LOGIN_MS_URL", "http://localhost");
        Environment.SetEnvironmentVariable("ORDER_MS_URL", "http://localhost");
        Environment.SetEnvironmentVariable("PAYMENT_MS_URL", "http://localhost");
    }

    [Fact]
    public async Task WhenRequestWithoutJwt_ReturnsUnauthorized_OnAuthorizedEndpoints()
    {
        using var client = _factory.CreateClient();

        var resp = await client.GetAsync("/Pedido/GetById?idPedido=1");

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task WhenJwtProvided_ButRoleMissing_ReturnsForbidden_OnRoleProtectedEndpoint()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwt(roles: ["999"]));

        var resp = await client.GetAsync("/Pedido/GetAll");

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task WhenJwtWithAdminRoleProvided_GetAll_ReturnsNotUnauthorizedOrForbidden()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwt(roles: [UsuarioRoles.Administrador]));

        var resp = await client.GetAsync("/Pedido/GetAll");

        Assert.False(resp.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task WhenJwtWithClientRoleProvided_CheckoutPedido_ReturnsNotUnauthorizedOrForbidden()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwt(roles: [UsuarioRoles.ClienteAnonimo]));

        // valorPedido is not marked [FromQuery] in the endpoint; send it both ways to avoid binder surprises.
        var resp = await client.PostAsync("/Pedido/CheckoutPedido?idPedido=1&valorPedido=100", content: null);

        Assert.False(resp.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task WhenJwtWithClientRoleProvided_CheckStatusPagamentoPedido_ReturnsNotUnauthorizedOrForbidden()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwt(roles: [UsuarioRoles.ClienteAnonimo]));

        var resp = await client.GetAsync("/Pedido/CheckStatusPagamentoPedido?idPedido=1");

        Assert.False(resp.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task WhenJwtWithClientRoleProvided_PutInformaPagamentoPedidoAprovado_ReturnsNotUnauthorizedOrForbidden()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwt(roles: [UsuarioRoles.ClienteAnonimo]));

        var resp = await client.PutAsync("/Pedido/InformaPagamentoPedidoAprovado?idPedido=1", content: null);

        Assert.False(resp.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task WhenJwtWithClientRoleProvided_PutInformaPagamentoPedidoNegado_ReturnsNotUnauthorizedOrForbidden()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwt(roles: [UsuarioRoles.ClienteAnonimo]));

        var resp = await client.PutAsync("/Pedido/InformaPagamentoPedidoNegado?idPedido=1", content: null);

        Assert.False(resp.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task WebhookEndpoint_AllowsAnonymous_AndReturnsDifferentOfUnauthorizedAndForbidden()
    {
        using var client = _factory.CreateClient();

        using var content = new StringContent("{\"any\":\"payload\"}", Encoding.UTF8, "application/json");
        var resp = await client.PostAsync("/Webhook/Pagamento?origem=MercadoPago&identificador=1", content);

        Assert.Equal(HttpStatusCode.InternalServerError, resp.StatusCode);
    }

    private static string CreateJwt(string[] roles)
    {
        var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(GetApiAuthenticationKey()));
        var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var claims = new List<System.Security.Claims.Claim>();
        foreach (var r in roles)
            claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, r));

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(5),
        signingCredentials: creds);

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GetApiAuthenticationKey()
    {
        return Environment.GetEnvironmentVariables()["API_AUTHENTICATION_KEY"]?.ToString() ?? "";
    }
}
