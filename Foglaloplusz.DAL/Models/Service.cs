using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foglaloplusz.DAL.Models
{
    /// <summary>
    /// Salon-scoped service definition. Exactly one of <see cref="OwnerId"/> or <see cref="ArtistId"/> is set: owner catalog vs artist-created.
    /// </summary>
    public class Service
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Price { get; set; }

        public int DurationInMinute { get; set; }

        public int SalonId { get; set; }
        public Salon Salon { get; set; } = null!;

        /// <summary>Set when the salon owner created this catalog entry; <see cref="ArtistId"/> must be null.</summary>
        public int? OwnerId { get; set; }
        public Owner? Owner { get; set; }

        /// <summary>Set when an artist created this service; <see cref="OwnerId"/> must be null.</summary>
        public int? ArtistId { get; set; }
        public Artist? Artist { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Offering> Offerings { get; set; } = new List<Offering>();
    }
}
