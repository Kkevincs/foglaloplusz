using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salonic.Models
{
    public class Guest
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Pwd { get; set; } = string.Empty;

        // Navigation: one Guest has many Appointments (1 : N)
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public string Role { get; set; } = "Guest";
    }
}

