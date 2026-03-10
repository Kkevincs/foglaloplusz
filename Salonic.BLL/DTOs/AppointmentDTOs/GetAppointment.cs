using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.DTOs.AppointmentDTOs
{
    public class GetAppointment
    {
        public int Id { get; set; }
        public int GuestId { get; set; }

        public int ArtistId { get; set; }

        public int ServiceId { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public string Notes { get; set; }

        public int DurationInMinutes { get; set; }
    }
}
