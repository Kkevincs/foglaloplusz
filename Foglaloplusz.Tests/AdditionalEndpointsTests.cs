using System.Net;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace Foglaloplusz.Tests;

/// <summary>További végpontok: elérhetőség, időpontok, lookup, munkaidő, naptár, CSV, törlés.</summary>
public class AdditionalEndpointsTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fx;
    private readonly ITestOutputHelper _out;

    public AdditionalEndpointsTests(ApiTestFixture fx, ITestOutputHelper output)
    {
        _fx = fx;
        _out = output;
    }

    [Fact]
    public async Task Availability_month_returns_ok_when_artist_offers_services()
    {
        using var client = _fx.Factory.CreateClient();
        var (_, serviceId, artistId, _, _) = await TestDataBuilder.CreateSalonWithLinkedCatalogServiceAsync(client);
        TestDataBuilder.SetBearer(client, null);

        var res = await client.PostJsonAsync("/api/availability/month", new
        {
            artistId,
            serviceIds = new[] { serviceId },
            year = 2026,
            month = 6,
        });
        res.EnsureSuccessLog(_out, "POST /api/availability/month");
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        TestOutputAssert.Equal(_out, JsonValueKind.Array, doc.RootElement.ValueKind, "availability month root JSON kind");
    }

    [Fact]
    public async Task Appointment_available_slots_returns_ok()
    {
        using var client = _fx.Factory.CreateClient();
        var (_, serviceId, artistId, _, _) = await TestDataBuilder.CreateSalonWithLinkedCatalogServiceAsync(client);
        TestDataBuilder.SetBearer(client, null);

        var res = await client.GetAsync(
            $"/api/appointment/available?artistId={artistId}&serviceIds={serviceId}&date=2026-06-15");
        res.EnsureSuccessLog(_out, "GET /api/appointment/available");
    }

    [Fact]
    public async Task Appointment_salon_list_for_owner_returns_ok()
    {
        using var client = _fx.Factory.CreateClient();
        var (salonId, _, _, ownerToken, _) = await TestDataBuilder.CreateSalonWithLinkedCatalogServiceAsync(client);
        TestDataBuilder.SetBearer(client, ownerToken);
        var res = await client.GetAsync($"/api/appointment/salon/{salonId}");
        res.EnsureSuccessLog(_out, $"GET /api/appointment/salon/{salonId}");
    }

    [Fact]
    public async Task Lookup_names_returns_ok_for_valid_ids()
    {
        using var client = _fx.Factory.CreateClient();
        var (salonId, serviceId, artistId, _, _) = await TestDataBuilder.CreateSalonWithLinkedCatalogServiceAsync(client);

        var gEmail = TestDataBuilder.UniqueEmail("guest");
        await client.PostJsonAsync("/api/register/guest", new { name = "Vendég", email = gEmail, pwd = "secret1" });
        var gToken = await TestDataBuilder.AuthenticateAsync(client, gEmail, "secret1", "guest");
        TestDataBuilder.SetBearer(client, gToken);
        var me = await client.GetAsync("/api/guest/me");
        me.EnsureSuccessLog(_out, "GET /api/guest/me");
        using var gDoc = JsonDocument.Parse(await me.Content.ReadAsStringAsync());
        var guestId = gDoc.RootElement.GetProperty("id").GetInt32();

        TestDataBuilder.SetBearer(client, null);
        var res = await client.PostJsonAsync("/api/lookup/names", new
        {
            artistId,
            guestId,
            serviceIds = new[] { serviceId },
        });
        res.EnsureSuccessLog(_out, "POST /api/lookup/names");
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        var hasArtist = doc.RootElement.TryGetProperty("artistName", out var artistEl);
        TestOutputAssert.True(_out, hasArtist, "lookup response has artistName", $"artistName raw: {artistEl}");
    }

    [Fact]
    public async Task Working_hours_post_unauthorized_without_artist_token()
    {
        using var client = _fx.Factory.CreateClient();
        TestDataBuilder.SetBearer(client, null);
        var res = await client.PostJsonAsync("/api/workinghours", new
        {
            date = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc),
            startTime = "09:00:00",
            endTime = "17:00:00",
        });
        TestOutputAssert.Equal(_out, HttpStatusCode.Unauthorized, res.StatusCode, "POST /api/workinghours unauthenticated");
    }

    [Fact]
    public async Task Working_hours_post_ok_for_artist()
    {
        using var client = _fx.Factory.CreateClient();
        var (_, _, _, _, artistToken) = await TestDataBuilder.CreateSalonWithLinkedCatalogServiceAsync(client);
        TestDataBuilder.SetBearer(client, artistToken);
        var res = await client.PostJsonAsync("/api/workinghours", new
        {
            date = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc),
            startTime = "09:00:00",
            endTime = "17:00:00",
        });
        res.EnsureSuccessLog(_out, "POST /api/workinghours as artist");
    }

    [Fact]
    public async Task Artist_list_all_authorized_for_guest()
    {
        using var client = _fx.Factory.CreateClient();
        var gEmail = TestDataBuilder.UniqueEmail("guest");
        await client.PostJsonAsync("/api/register/guest", new { name = "G", email = gEmail, pwd = "secret1" });
        var token = await TestDataBuilder.AuthenticateAsync(client, gEmail, "secret1", "guest");
        TestDataBuilder.SetBearer(client, token);
        var res = await client.GetAsync("/api/Artist");
        res.EnsureSuccessLog(_out, "GET /api/Artist as guest");
    }

    [Fact]
    public async Task Owner_week_schedule_returns_ok()
    {
        using var client = _fx.Factory.CreateClient();
        var (salonId, _, artistId, ownerToken, _) = await TestDataBuilder.CreateSalonWithLinkedCatalogServiceAsync(client);
        TestDataBuilder.SetBearer(client, ownerToken);
        var res = await client.GetAsync(
            $"/api/salon/{salonId}/artists/{artistId}/schedule?weekStart=2026-06-08");
        res.EnsureSuccessLog(_out, "GET owner week schedule");
    }

    [Fact]
    public async Task Salon_services_for_artist_public_returns_ok()
    {
        using var client = _fx.Factory.CreateClient();
        var (salonId, _, artistId, _, _) = await TestDataBuilder.CreateSalonWithLinkedCatalogServiceAsync(client);
        TestDataBuilder.SetBearer(client, null);
        var res = await client.GetAsync($"/api/salon/{salonId}/services/for-artist/{artistId}");
        res.EnsureSuccessLog(_out, "GET services/for-artist (public)");
    }

    [Fact]
    public async Task Owner_delete_unknown_catalog_service_returns_404()
    {
        using var client = _fx.Factory.CreateClient();
        var (salonId, _, _, ownerToken, _) = await TestDataBuilder.CreateSalonWithLinkedCatalogServiceAsync(client);
        TestDataBuilder.SetBearer(client, ownerToken);
        var res = await client.DeleteAsync($"/api/salon/{salonId}/services/999999");
        TestOutputAssert.Equal(_out, HttpStatusCode.NotFound, res.StatusCode, "DELETE unknown catalog service");
    }

    [Fact]
    public async Task Owner_csv_import_creates_row()
    {
        using var client = _fx.Factory.CreateClient();
        var (salonId, ownerToken) = await TestDataBuilder.RegisterOwnerAndCreateSalonAsync(client);
        TestDataBuilder.SetBearer(client, ownerToken);
        var csv = "servicename;duration;description;price\nCsvSvc;40;;2500\n";
        var res = await client.PostJsonAsync($"/api/salon/{salonId}/services/import", new { csvText = csv });
        res.EnsureSuccessLog(_out, "POST services/import (owner CSV)");
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        var createdLen = doc.RootElement.GetProperty("created").GetArrayLength();
        TestOutputAssert.True(_out, createdLen >= 1, "CSV import created count", $"created.Length = {createdLen}");
    }

    [Fact]
    public async Task Guest_cancel_unknown_appointment_returns_404()
    {
        using var client = _fx.Factory.CreateClient();
        var gEmail = TestDataBuilder.UniqueEmail("guest");
        await client.PostJsonAsync("/api/register/guest", new { name = "G", email = gEmail, pwd = "secret1" });
        var token = await TestDataBuilder.AuthenticateAsync(client, gEmail, "secret1", "guest");
        TestDataBuilder.SetBearer(client, token);
        var res = await client.DeleteAsync("/api/appointment/guest/999999");
        TestOutputAssert.Equal(_out, HttpStatusCode.NotFound, res.StatusCode, "DELETE unknown guest appointment");
    }

    [Fact]
    public async Task Artist_get_salon_service_by_id_returns_ok_when_linked()
    {
        using var client = _fx.Factory.CreateClient();
        var (salonId, serviceId, _, _, artistToken) = await TestDataBuilder.CreateSalonWithLinkedCatalogServiceAsync(client);
        TestDataBuilder.SetBearer(client, artistToken);
        var res = await client.GetAsync($"/api/salon/{salonId}/services/{serviceId}");
        res.EnsureSuccessLog(_out, "GET salon service by id (artist, linked)");
    }

    [Fact]
    public async Task Artist_csv_import_creates_service()
    {
        using var client = _fx.Factory.CreateClient();
        var (salonId, _, _, _, artistToken) = await TestDataBuilder.CreateSalonWithLinkedCatalogServiceAsync(client);
        TestDataBuilder.SetBearer(client, artistToken);
        var csv = "servicename;duration;description;price\nImported;25;;1500\n";
        var res = await client.PostJsonAsync("/api/Artist/services/import", new { csvText = csv });
        res.EnsureSuccessLog(_out, "POST Artist/services/import");
    }

    [Fact]
    public async Task Working_hours_get_range_requires_artist()
    {
        using var client = _fx.Factory.CreateClient();
        TestDataBuilder.SetBearer(client, null);
        var res = await client.GetAsync("/api/workinghours/range");
        TestOutputAssert.Equal(_out, HttpStatusCode.Unauthorized, res.StatusCode, "GET /api/workinghours/range unauthenticated");
    }
}
