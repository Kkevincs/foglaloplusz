using System.Net;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace Foglaloplusz.Tests;

/// <summary>Tulaj → szalon → katalógus → szakember link; tulaj frissítés láncolása.</summary>
public class OwnerSalonCatalogArtistFlowTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fx;
    private readonly ITestOutputHelper _out;

    public OwnerSalonCatalogArtistFlowTests(ApiTestFixture fx, ITestOutputHelper output)
    {
        _fx = fx;
        _out = output;
    }

    [Fact]
    public async Task Owner_my_salons_and_public_salon_endpoints_after_seed()
    {
        using var client = _fx.Factory.CreateClient();
        var (salonId, ownerToken) = await TestDataBuilder.RegisterOwnerAndCreateSalonAsync(client);

        TestDataBuilder.SetBearer(client, ownerToken);
        var my = await client.GetAsync("/api/salon/my");
        my.EnsureSuccessLog(_out, "GET /api/salon/my");

        TestDataBuilder.SetBearer(client, null);
        var exists = await client.GetAsync($"/api/salon/{salonId}/exists");
        TestOutputAssert.Equal(_out, HttpStatusCode.NoContent, exists.StatusCode, $"GET /api/salon/{salonId}/exists");

        var profile = await client.GetAsync($"/api/salon/{salonId}/public-profile");
        profile.EnsureSuccessLog(_out, $"GET /api/salon/{salonId}/public-profile");

        var artists = await client.GetAsync($"/api/salon/{salonId}/public-artists");
        artists.EnsureSuccessLog(_out, $"GET /api/salon/{salonId}/public-artists");
    }

    [Fact]
    public async Task Owner_crud_catalog_service_and_artist_sees_updated_name_after_link()
    {
        using var client = _fx.Factory.CreateClient();
        var (salonId, ownerToken) = await TestDataBuilder.RegisterOwnerAndCreateSalonAsync(client);

        TestDataBuilder.SetBearer(client, ownerToken);
        var createSvc = await client.PostJsonAsync($"/api/salon/{salonId}/services", new
        {
            name = "Vágás",
            description = "Alap",
            price = 3000,
            durationInMinute = 30,
        });
        createSvc.EnsureSuccessLog(_out, "POST catalog service");
        using var createdDoc = JsonDocument.Parse(await createSvc.Content.ReadAsStringAsync());
        var serviceId = createdDoc.RootElement.GetProperty("id").GetInt32();

        var catalog = await client.GetAsync($"/api/salon/{salonId}/services/catalog");
        catalog.EnsureSuccessLog(_out, "GET services/catalog");

        var (_, artistToken) = await TestDataBuilder.RegisterArtistAsync(client, salonId);
        TestDataBuilder.SetBearer(client, artistToken);
        var link = await client.PostJsonAsync("/api/Artist/services/link", new { serviceId });
        link.EnsureSuccessLog(_out, "POST Artist/services/link");

        var mySvc = await client.GetAsync("/api/Artist/services");
        mySvc.EnsureSuccessLog(_out, "GET Artist/services (before owner update)");
        using var listBefore = JsonDocument.Parse(await mySvc.Content.ReadAsStringAsync());
        var firstName = listBefore.RootElement[0].GetProperty("name").GetString();
        TestOutputAssert.Equal(_out, "Vágás", firstName, "linked service name (before update)");

        TestDataBuilder.SetBearer(client, ownerToken);
        var update = await client.PutJsonAsync($"/api/salon/{salonId}/services/{serviceId}", new
        {
            name = "Vágás prémium",
            description = "Frissített",
            price = 3500,
            durationInMinute = 45,
        });
        update.EnsureSuccessLog(_out, "PUT catalog service");

        TestDataBuilder.SetBearer(client, artistToken);
        var mySvc2 = await client.GetAsync("/api/Artist/services");
        mySvc2.EnsureSuccessLog(_out, "GET Artist/services (after owner update)");
        using var listAfter = JsonDocument.Parse(await mySvc2.Content.ReadAsStringAsync());
        TestOutputAssert.Equal(_out, "Vágás prémium", listAfter.RootElement[0].GetProperty("name").GetString(), "service name");
        TestOutputAssert.Equal(_out, 45, listAfter.RootElement[0].GetProperty("durationInMinute").GetInt32(), "durationInMinute");
        TestOutputAssert.Equal(_out, 3500, listAfter.RootElement[0].GetProperty("price").GetInt32(), "price");
    }

    [Fact]
    public async Task Owner_salon_artists_list_requires_owner_role()
    {
        using var client = _fx.Factory.CreateClient();
        var (salonId, _) = await TestDataBuilder.RegisterOwnerAndCreateSalonAsync(client);
        TestDataBuilder.SetBearer(client, null);
        var res = await client.GetAsync($"/api/salon/{salonId}/artists");
        TestOutputAssert.Equal(_out, HttpStatusCode.Unauthorized, res.StatusCode, "GET /artists without token");
    }

    [Fact]
    public async Task Artist_creates_own_service_and_updates_it()
    {
        using var client = _fx.Factory.CreateClient();
        var (salonId, _) = await TestDataBuilder.RegisterOwnerAndCreateSalonAsync(client);
        var (_, artistToken) = await TestDataBuilder.RegisterArtistAsync(client, salonId);
        TestDataBuilder.SetBearer(client, artistToken);

        var create = await client.PostJsonAsync("/api/Artist/services", new
        {
            name = "Saját kezelés",
            description = "",
            price = 5000,
            durationInMinute = 60,
        });
        create.EnsureSuccessLog(_out, "POST Artist/services");
        using var doc = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var sid = doc.RootElement.GetProperty("id").GetInt32();

        var update = await client.PutJsonAsync($"/api/Artist/services/{sid}", new
        {
            name = "Saját kezelés XL",
            description = "Új",
            price = 5500,
            durationInMinute = 90,
        });
        update.EnsureSuccessLog(_out, "PUT Artist/services");
        using var udoc = JsonDocument.Parse(await update.Content.ReadAsStringAsync());
        TestOutputAssert.Equal(_out, "Saját kezelés XL", udoc.RootElement.GetProperty("name").GetString(), "updated name");
    }

    [Fact]
    public async Task Owner_me_and_put_profile()
    {
        using var client = _fx.Factory.CreateClient();
        var ownerEmail = TestDataBuilder.UniqueEmail("ow");
        const string pwd = "secret1";
        (await client.PostJsonAsync("/api/register/owner", new { name = "O", email = ownerEmail, pwd })).EnsureSuccessLog(_out, "register owner");
        var token = await TestDataBuilder.AuthenticateAsync(client, ownerEmail, pwd, "owner");
        TestDataBuilder.SetBearer(client, token);
        var me = await client.GetAsync("/api/owner/me");
        me.EnsureSuccessLog(_out, "GET /api/owner/me");
        using var meDoc = JsonDocument.Parse(await me.Content.ReadAsStringAsync());
        var nameWas = meDoc.RootElement.GetProperty("name").GetString();

        var expectedName = nameWas + " X";
        var put = await client.PutJsonAsync("/api/owner/me", new
        {
            name = expectedName,
            email = ownerEmail,
        });
        put.EnsureSuccessLog(_out, "PUT /api/owner/me");

        var me2 = await client.GetAsync("/api/owner/me");
        me2.EnsureSuccessLog(_out, "GET /api/owner/me after PUT");
        using var me2Doc = JsonDocument.Parse(await me2.Content.ReadAsStringAsync());
        var nameNow = me2Doc.RootElement.GetProperty("name").GetString();
        TestOutputAssert.Equal(_out, expectedName, nameNow, "owner name persisted");
    }

    [Fact]
    public async Task Guest_me_requires_auth_or_wrong_role()
    {
        using var client = _fx.Factory.CreateClient();
        var res = await client.GetAsync("/api/guest/me");
        TestOutputAssert.Equal(_out, HttpStatusCode.Unauthorized, res.StatusCode, "GET /api/guest/me unauthenticated");
    }
}
