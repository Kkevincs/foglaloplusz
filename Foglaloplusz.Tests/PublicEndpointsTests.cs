using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace Foglaloplusz.Tests;

public class PublicEndpointsTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fx;
    private readonly ITestOutputHelper _out;

    public PublicEndpointsTests(ApiTestFixture fx, ITestOutputHelper output)
    {
        _fx = fx;
        _out = output;
    }

    [Fact]
    public async Task Salon_public_list_returns_ok()
    {
        using var client = _fx.Factory.CreateClient();
        var res = await client.GetAsync("/api/salon/public");
        res.EnsureSuccessLog(_out, "GET /api/salon/public");
    }

    [Fact]
    public async Task Salon_directory_returns_ok()
    {
        using var client = _fx.Factory.CreateClient();
        var res = await client.GetAsync("/api/salon/directory");
        res.EnsureSuccessLog(_out, "GET /api/salon/directory");
    }

    [Fact]
    public async Task Salon_exists_unknown_returns_404()
    {
        using var client = _fx.Factory.CreateClient();
        var res = await client.GetAsync("/api/salon/999999/exists");
        TestOutputAssert.Equal(_out, HttpStatusCode.NotFound, res.StatusCode, "GET /api/salon/999999/exists");
    }

    [Fact]
    public async Task Salon_public_profile_unknown_returns_404()
    {
        using var client = _fx.Factory.CreateClient();
        var res = await client.GetAsync("/api/salon/999999/public-profile");
        TestOutputAssert.Equal(_out, HttpStatusCode.NotFound, res.StatusCode, "GET /api/salon/999999/public-profile");
    }

    [Fact]
    public async Task Salon_public_artists_unknown_salon_returns_404()
    {
        using var client = _fx.Factory.CreateClient();
        var res = await client.GetAsync("/api/salon/999999/public-artists");
        TestOutputAssert.Equal(_out, HttpStatusCode.NotFound, res.StatusCode, "GET /api/salon/999999/public-artists");
    }

    [Fact]
    public async Task Salon_services_list_unknown_returns_empty_or_ok()
    {
        using var client = _fx.Factory.CreateClient();
        var res = await client.GetAsync("/api/salon/999999/services");
        res.EnsureSuccessLog(_out, "GET /api/salon/999999/services");
    }
}
