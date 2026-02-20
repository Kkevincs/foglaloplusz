using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salonic.Models
{
    public class Appointment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int GuestId { get; set; }
        public Guest Guest { get; set; } = null!;

        public int WorkerId { get; set; }
        public Worker Worker { get; set; } = null!;

        public int ServiceId { get; set; }
        public Service Service { get; set; } = null!;
        [Required]
        public int Year { get; set; }
        [Required]
        public int Month { get; set; }
        [Required]
        public int Day { get; set; }
        [Required]
        public int Hour { get; set; }
        [Required]
        public int Minute { get; set; }
        [Required]
        public string Notes { get; set; }
        [Required]
        public int DurationInMinutes { get; set; }

        //[ForeignKey(nameof(UserId))]
        //public User? User { get; set; }
        //[ForeignKey(nameof(ServiceId))]
        //public Service? Service { get; set; }
    }
}
