using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Salonic.DAL.Models
{
    public class Service
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; }

        public int Price { get; set; }

        public int DurationInMinute { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Navigation: many-to-many with Worker (EF creates the junction table automatically)
        public ICollection<Artist> Artists { get; set; } = new List<Artist>();
    }   
}
