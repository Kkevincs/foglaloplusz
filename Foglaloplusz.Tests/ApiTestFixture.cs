namespace Foglaloplusz.Tests;

/// <summary>Osztályonként egy API host + egy InMemory adatbázis. Minden tesztben: <c>using var client = _fixture.Factory.CreateClient();</c></summary>
public class ApiTestFixture : IDisposable
{
    public TestWebApplicationFactory Factory { get; } = new();

    public void Dispose() => Factory.Dispose();
}
