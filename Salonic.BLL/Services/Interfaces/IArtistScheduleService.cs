using Salonic.BLL.DTOs.AppointmentDTOs;

namespace Salonic.BLL.Services.Interfaces
{
    public interface IArtistScheduleService
    {
        /// <summary>
        /// Builds owner-visible week grid when the salon belongs to the owner and the artist belongs to that salon.
        /// Returns null when the owner does not own the salon or the artist is not in that salon.
        /// </summary>
        Task<ArtistWeekScheduleDTO?> GetWeekForOwnerAsync(
            int ownerId,
            int salonId,
            int artistId,
            DateTime weekStartMonday,
            CancellationToken cancellationToken = default);
    }
}
