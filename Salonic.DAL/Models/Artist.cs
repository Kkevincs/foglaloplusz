using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.DAL.Models
{
    public class Artist
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Pwd { get; set; }

        public string Description { get; set; }

        // Navigation: one Worker has many Appointments (1 : N)
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Navigation: many-to-many with Service (EF creates the junction table automatically)
        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
