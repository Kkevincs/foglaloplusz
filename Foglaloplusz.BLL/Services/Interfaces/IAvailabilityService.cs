using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foglaloplusz.BLL.Services.Interfaces
{
    public interface IAvailabilityService
    {
        Task<List<DayAvailabilityDto>> GetMonthAvailability(MonthAvailabilityRequestDto request);
    }
}
