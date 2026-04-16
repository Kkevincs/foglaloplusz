using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foglaloplusz.BLL.DTOs.AppointmentDTOs
{
    public class WorkingHoursDTO
    {
        public DateTime Date { get; set; }

        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
