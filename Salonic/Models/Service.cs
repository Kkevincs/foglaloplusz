using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Salonic.Models
{
    public class Service
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; }

        public int Price { get; set; }

        // Navigation: one Service has many Appointments (1 : N)
        [JsonIgnore]
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Navigation: many-to-many with Worker (EF creates the junction table automatically)
        [JsonIgnore]
        public ICollection<Worker> Workers { get; set; } = new List<Worker>();
    }
}

