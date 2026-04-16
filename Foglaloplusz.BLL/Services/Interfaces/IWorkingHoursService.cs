using Foglaloplusz.BLL.DTOs.AppointmentDTOs;

namespace Foglaloplusz.BLL.Services.Interfaces
{
    public interface IWorkingHoursService
    {
        Task Set(int artistId, WorkingHoursDTO dto);
        Task SetSchedule(int artistId, SetWorkingHoursScheduleDTO dto);
        Task<List<WorkingHoursDTO>> GetInRange(int artistId, WorkingHoursRangeRequestDTO dto);
    }
}
