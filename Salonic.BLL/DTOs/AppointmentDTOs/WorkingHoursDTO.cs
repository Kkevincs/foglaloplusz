using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.DTOs.AppointmentDTOs
{
    public class WorkingHoursDTO
    {
        public int ArtistId { get; set; }

        public DateTime Date { get; set; }

        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
