using Foglaloplusz.DAL.Models.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foglaloplusz.DAL.Models
{
    public class Artist : IUserModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Pwd { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public ICollection<Offering> Offerings { get; set; } = new List<Offering>();

        public int SalonId { get; set; }
        public Salon Salon { get; set; } = null!;

        public string Role { get; set; } = "Artist";
    }
}
