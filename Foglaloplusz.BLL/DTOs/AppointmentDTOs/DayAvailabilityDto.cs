using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foglaloplusz.BLL.DTOs.AppointmentDTOs
{
    public class DayAvailabilityDto
    {
        public DateTime Date { get; set; }
        public bool HasAvailableAppointments { get; set; }
    }
}
