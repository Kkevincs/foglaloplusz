using Foglaloplusz.BLL.DTOs.AppointmentDTOs;

namespace Foglaloplusz.BLL.Services.Interfaces;

public interface IAvailabilityService
{
    Task<List<DayAvailabilityDto>> GetMonthAvailability(MonthAvailabilityRequestDto? request);
}
