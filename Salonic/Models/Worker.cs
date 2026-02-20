using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salonic.Models
{
    public class Worker
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Pwd { get; set; }

        // Navigation: one Worker has many Appointments (1 : N)
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Navigation: many-to-many with Service (EF creates the junction table automatically)
        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
