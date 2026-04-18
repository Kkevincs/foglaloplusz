using Microsoft.AspNetCore.Mvc;

namespace Foglaloplusz.presentation.Infrastructure;

/// <summary>Közös JWT „sub” feloldás a vezérlőknek (DRY).</summary>
public abstract class ApiControllerBase : ControllerBase
{
    protected bool TryGetUserIdFromSub(out int userId)
    {
        userId = 0;
        var raw = User.FindFirst("sub")?.Value;
        return raw != null && int.TryParse(raw, out userId);
    }
}
