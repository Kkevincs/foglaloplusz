using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Salonic.Models
{
    public class Appointment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int GuestId { get; set; }
        [JsonIgnore]
        [BindNever]
        public Guest? Guest { get; set; }

        public int WorkerId { get; set; }
        [JsonIgnore]
        [BindNever]
        public Worker? Worker { get; set; }

        public int ServiceId { get; set; }
        [JsonIgnore]
        [BindNever]
        public Service? Service { get; set; }
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
        public string Notes { get; set; } = string.Empty;
        [Required]
        public int DurationInMinutes { get; set; }

        //[ForeignKey(nameof(UserId))]
        //public User? User { get; set; }
        //[ForeignKey(nameof(ServiceId))]
        //public Service? Service { get; set; }
    }
}
