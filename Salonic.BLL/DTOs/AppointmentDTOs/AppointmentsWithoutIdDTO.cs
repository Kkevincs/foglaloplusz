using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.DTOs.AppointmentDTOs
{
    public class AppointmentsWithoutIdDTO
    {
        public int GuestId { get; set; }
        public int ArtistId { get; set; }
        public int SalonId { get; set; }
        public List<int> ServiceIds { get; set; } = new();
        public DateTime StartTime { get; set; }
        public string Notes { get; set; }
    }
}
