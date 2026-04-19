namespace Foglaloplusz.DAL;

/// <summary>
/// Npgsql 6+: <c>timestamp with time zone</c> parameters must use <see cref="DateTimeKind.Utc"/>.
/// JSON / model binder often yields <see cref="DateTimeKind.Unspecified"/>.
/// </summary>
public static class DateTimeNpgsqlExtensions
{
    public static DateTime ToUtcInstant(this DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };

    /// <summary>Calendar day at 00:00 UTC (booking / working-hours day boundaries).</summary>
    public static DateTime ToUtcDate(this DateTime value) =>
        DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
}
