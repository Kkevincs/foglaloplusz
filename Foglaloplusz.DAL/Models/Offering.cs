using System.ComponentModel.DataAnnotations.Schema;

namespace Foglaloplusz.DAL.Models
{
    /// <summary>
    /// Which services an artist offers (display/booking). <see cref="IsImported"/> is true when the <see cref="Service"/> was created by the salon owner; false when the artist created the service.
    /// </summary>
    public class Offering
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ServiceId { get; set; }
        public Service Service { get; set; } = null!;

        public int ArtistId { get; set; }
        public Artist Artist { get; set; } = null!;

        public bool IsImported { get; set; }
    }
}
