using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace Foglaloplusz.Tests;

public class AuthAndRegisterTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fx;
    private readonly ITestOutputHelper _out;

    public AuthAndRegisterTests(ApiTestFixture fx, ITestOutputHelper output)
    {
        _fx = fx;
        _out = output;
    }

    [Fact]
    public async Task Register_guest_then_authenticate_ok()
    {
        using var client = _fx.Factory.CreateClient();
        var email = TestDataBuilder.UniqueEmail("g");
        var reg = await client.PostJsonAsync("/api/register/guest", new { name = "Vendég", email, pwd = "secret1" });
        reg.EnsureSuccessLog(_out, "POST /api/register/guest");

        var token = await TestDataBuilder.AuthenticateAsync(client, email, "secret1", "guest");
        TestOutputAssert.NotEmpty(_out, token, "JWT after authenticate");
    }

    [Fact]
    public async Task Authenticate_wrong_password_returns_401()
    {
        using var client = _fx.Factory.CreateClient();
        var email = TestDataBuilder.UniqueEmail("g2");
        var reg = await client.PostJsonAsync("/api/register/guest", new { name = "V", email, pwd = "secret1" });
        reg.EnsureSuccessLog(_out, "POST /api/register/guest");

        var res = await client.PostJsonAsync("/api/authentication/authenticate", new
        {
            email,
            pwd = "wrong",
            role = "guest",
        });
        TestOutputAssert.Equal(_out, HttpStatusCode.Unauthorized, res.StatusCode, "authenticate wrong password");
    }

    [Fact]
    public async Task Authenticate_artist_without_salonId_returns_400()
    {
        using var client = _fx.Factory.CreateClient();
        var res = await client.PostJsonAsync("/api/authentication/authenticate", new
        {
            email = "a@test.local",
            pwd = "x",
            role = "artist",
        });
        TestOutputAssert.Equal(_out, HttpStatusCode.BadRequest, res.StatusCode, "artist auth without salonId");
    }
}
