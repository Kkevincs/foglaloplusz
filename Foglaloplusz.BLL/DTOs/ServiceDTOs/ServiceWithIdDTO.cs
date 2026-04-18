namespace Foglaloplusz.BLL.DTOs.ServiceDTOs
{
    public class ServiceWithIdDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Price { get; set; }
        public int DurationInMinute { get; set; }

        /// <summary>Service row: owner-defined catalog entry.</summary>
        public int? OwnerId { get; set; }

        /// <summary>Service row: artist who created this service (not an offering).</summary>
        public int? OwnerArtistId { get; set; }

        /// <summary>When listing via artist offerings: the offering row id.</summary>
        public int? OfferingId { get; set; }

        /// <summary>True if taken from owner catalog via <see cref="Offering"/>.</summary>
        public bool? IsImported { get; set; }

        public int SalonId { get; set; }
    }
}
