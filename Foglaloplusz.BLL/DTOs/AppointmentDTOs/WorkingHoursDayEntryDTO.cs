namespace Foglaloplusz.BLL.DTOs.AppointmentDTOs
{
    /// <summary>
    /// Use clock strings like "09:00" or "09:00:00" (same as single-day WorkingHoursDTO).
    /// System.Text.Json does not map "09:00:00" to TimeSpan?, so these are strings.
    /// </summary>
    public class WorkingHoursDayEntryDTO
    {
        public string? Start { get; set; }
        public string? End { get; set; }
    }
}
