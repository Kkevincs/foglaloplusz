namespace Foglaloplusz.BLL.Exceptions;

/// <summary>Érvénytelen kérés vagy üzleti szabály megsértése — HTTP 400.</summary>
public sealed class AppValidationException : Exception
{
    public AppValidationException(string message) : base(message)
    {
    }

    public AppValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>Az erőforrás nem található — HTTP 404.</summary>
public sealed class AppNotFoundException : Exception
{
    public AppNotFoundException(string message) : base(message)
    {
    }
}

/// <summary>A művelethez nincs jogosultság — HTTP 403.</summary>
public sealed class AppForbiddenException : Exception
{
    public AppForbiddenException(string message) : base(message)
    {
    }
}
