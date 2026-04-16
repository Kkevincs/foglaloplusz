using Foglaloplusz.DAL.Models;

namespace Foglaloplusz.DAL.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<List<Appointment>> ListByGuestIdAsync(int guestId);
        Task<List<Appointment>> ListByArtistIdAsync(int artistId);
        Task<List<Appointment>> ListBySalonIdAsync(int salonId);
        Task<List<Appointment>> ListByArtistAndDateAsync(int artistId, DateTime date);

        /// <param name="rangeEndExclusive">Instant at start of first day after the week (weekStart + 7 days).</param>
        Task<List<Appointment>> ListByArtistAndDateRangeAsync(int artistId, DateTime rangeStart, DateTime rangeEndExclusive);
        Task<bool> HasOverlapAsync(int artistId, DateTime startTime, DateTime endTime);
        Task AddAsync(Appointment appointment);

        /// <summary>Deletes an appointment if it belongs to the guest. Returns false if not found.</summary>
        Task<bool> DeleteForGuestAsync(int guestId, int appointmentId);
    }
}
