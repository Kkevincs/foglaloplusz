namespace Salonic.BLL.DTOs.AppointmentDTOs
{
    public class SetWorkingHoursScheduleDTO
    {
        public DateOnly StartDate { get; set; }
        public List<WorkingHoursDayEntryDTO> Days { get; set; } = new();
    }
}
