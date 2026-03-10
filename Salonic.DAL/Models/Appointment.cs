using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Salonic.DAL.Models
{
    public class Appointment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int GuestId { get; set; }
        public Guest Guest { get; set; }

        public int ArtistId { get; set; }
        public Artist Artist { get; set; }

        public int ServiceId { get; set; }
        public Service Service { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public string Notes { get; set; }

        public int DurationInMinutes { get; set; }
    }
}
