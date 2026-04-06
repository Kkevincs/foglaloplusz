using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salonic.DAL.Models
{
    public class Service
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public int Price { get; set; }

        public int DurationInMinute { get; set; }

        public int SalonId { get; set; }
        public Salon Salon { get; set; } = null!;

        
        public int CreatedByArtistId { get; set; }
        public Artist CreatedByArtist { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public ICollection<Artist> Artists { get; set; } = new List<Artist>();
    }
}
