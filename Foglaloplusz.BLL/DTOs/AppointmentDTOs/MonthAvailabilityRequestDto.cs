using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foglaloplusz.BLL.DTOs.AppointmentDTOs
{
    public class MonthAvailabilityRequestDto
    {
        public int ArtistId { get; set; }
        public int[] ServiceIds { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
