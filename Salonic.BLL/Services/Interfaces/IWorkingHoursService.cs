using Salonic.BLL.DTOs.AppointmentDTOs;

namespace Salonic.BLL.Services.Interfaces
{
    public interface IWorkingHoursService
    {
        Task Set(int artistId, WorkingHoursDTO dto);
        Task SetSchedule(int artistId, SetWorkingHoursScheduleDTO dto);
    }
}
