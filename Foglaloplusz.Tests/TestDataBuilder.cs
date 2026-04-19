using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Foglaloplusz.Tests;

internal static class TestDataBuilder
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static string UniqueEmail(string prefix) => $"{prefix}_{Guid.NewGuid():N}@test.local";

    public static async Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string url, object body)
    {
        var json = JsonSerializer.Serialize(body, JsonOpts);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PostAsync(url, content);
    }

    public static async Task<HttpResponseMessage> PutJsonAsync(this HttpClient client, string url, object body)
    {
        var json = JsonSerializer.Serialize(body, JsonOpts);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PutAsync(url, content);
    }

    public static async Task<HttpResponseMessage> PatchJsonAsync(this HttpClient client, string url, object body)
    {
        var json = JsonSerializer.Serialize(body, JsonOpts);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PatchAsync(url, content);
    }

    public static void SetBearer(HttpClient client, string? token)
    {
        client.DefaultRequestHeaders.Authorization = token == null
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task<string?> AuthenticateAsync(HttpClient client, string email, string pwd, string role, int? salonId = null)
    {
        SetBearer(client, null);
        var body = new Dictionary<string, object?>
        {
            ["email"] = email,
            ["pwd"] = pwd,
            ["role"] = role,
        };
        if (salonId.HasValue)
            body["salonId"] = salonId.Value;

        var res = await client.PostJsonAsync("/api/authentication/authenticate", body);
        if (!res.IsSuccessStatusCode)
            return null;
        var text = (await res.Content.ReadAsStringAsync()).Trim();
        if (string.IsNullOrEmpty(text))
            return null;
        // Ok(string) gyakran text/plain nyers JWT; lehet JSON string is ("…").
        if (text.StartsWith('"'))
            return JsonSerializer.Deserialize<string>(text, JsonOpts);
        return text;
    }

    public static async Task<(int salonId, string ownerToken)> RegisterOwnerAndCreateSalonAsync(HttpClient client)
    {
        var ownerEmail = UniqueEmail("owner");
        const string pwd = "secret1";
        var reg = await client.PostJsonAsync("/api/register/owner", new
        {
            name = "Teszt Tulaj",
            email = ownerEmail,
            pwd,
        });
        reg.EnsureSuccessStatusCode();

        var ownerToken = await AuthenticateAsync(client, ownerEmail, pwd, "owner")
            ?? throw new InvalidOperationException("owner auth failed");

        SetBearer(client, ownerToken);
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("Teszt Szalon"), "name");
        form.Add(new StringContent(ownerEmail), "email");
        form.Add(new StringContent("theme-pink"), "theme");
        form.Add(new StringContent("Leírás"), "description");
        var salonRes = await client.PostAsync("/api/salon", form);
        salonRes.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await salonRes.Content.ReadAsStringAsync());
        var salonId = doc.RootElement.GetProperty("id").GetInt32();
        return (salonId, ownerToken);
    }

    public static async Task<(int artistId, string artistToken)> RegisterArtistAsync(
        HttpClient client,
        int salonId,
        string? email = null)
    {
        SetBearer(client, null);
        email ??= UniqueEmail("artist");
        const string pwd = "secret1";
        var reg = await client.PostJsonAsync("/api/register/artist", new
        {
            name = "Teszt Szakember",
            email,
            pwd,
            salonId,
            description = "Bio",
        });
        reg.EnsureSuccessStatusCode();

        var artistToken = await AuthenticateAsync(client, email, pwd, "artist", salonId)
            ?? throw new InvalidOperationException("artist auth failed");

        SetBearer(client, artistToken);
        var me = await client.GetAsync("/api/Artist/me");
        me.EnsureSuccessStatusCode();
        using var meDoc = JsonDocument.Parse(await me.Content.ReadAsStringAsync());
        var artistId = meDoc.RootElement.GetProperty("id").GetInt32();
        return (artistId, artistToken);
    }

    /// <summary>Tulaj katalógus + egy szolgáltatás + szakember, aki felveszi (link).</summary>
    public static async Task<(int salonId, int serviceId, int artistId, string ownerToken, string artistToken)> CreateSalonWithLinkedCatalogServiceAsync(
        HttpClient client)
    {
        var (salonId, ownerToken) = await RegisterOwnerAndCreateSalonAsync(client);
        SetBearer(client, ownerToken);
        var createSvc = await client.PostJsonAsync($"/api/salon/{salonId}/services", new
        {
            name = "Cut",
            description = "",
            price = 2000,
            durationInMinute = 30,
        });
        createSvc.EnsureSuccessStatusCode();
        using var d = JsonDocument.Parse(await createSvc.Content.ReadAsStringAsync());
        var serviceId = d.RootElement.GetProperty("id").GetInt32();

        SetBearer(client, null);
        var (artistId, artistToken) = await RegisterArtistAsync(client, salonId);
        SetBearer(client, artistToken);
        var link = await client.PostJsonAsync("/api/Artist/services/link", new { serviceId });
        link.EnsureSuccessStatusCode();
        return (salonId, serviceId, artistId, ownerToken, artistToken);
    }
}
