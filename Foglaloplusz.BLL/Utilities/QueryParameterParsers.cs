namespace Foglaloplusz.BLL.Utilities;

/// <summary>HTTP lekérdezésből származó opcionális szűrők feldolgozása (DRY).</summary>
public static class QueryParameterParsers
{
    /// <summary>Vesszővel elválasztott pozitív egészek (pl. szolgáltatás-azonosítók). Üres/null esetén null.</summary>
    public static int[]? ParseOptionalPositiveIntList(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
            return null;

        var parsed = csv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var n) ? n : 0)
            .Where(n => n > 0)
            .Distinct()
            .ToArray();

        return parsed.Length > 0 ? parsed : null;
    }
}
