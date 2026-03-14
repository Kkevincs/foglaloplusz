using Salonic.DAL.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.DAL.Models
{
    public class Artist : IUserModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Pwd { get; set; }

        public string Description { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public ICollection<Service> Services { get; set; } = new List<Service>();
        public int SalonId { get; set; }
        public Salon Salon { get; set; }

        public string Role { get; set; } = "Artist";
    }
}
