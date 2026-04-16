using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foglaloplusz.DAL.Models
{
    public class Salon
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }

        public ICollection<Artist> Artists { get; set; } = new List<Artist>();

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public int OwnerId { get; set; }
        public Owner Owner { get; set; }

        public string Theme { get; set; }

        public string LogoUrl { get; set; }
    }
}
