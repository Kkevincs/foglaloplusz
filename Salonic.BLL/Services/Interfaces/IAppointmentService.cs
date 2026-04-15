using Salonic.BLL.DTOs.AppointmentDTOs;

namespace Salonic.BLL.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<List<AppointmentsWithIdDTO>> ListForGuestAsync(int guestId);
        Task<List<AppointmentsWithIdDTO>> ListForArtistAsync(int artistId);
        Task<List<AppointmentsWithIdDTO>> ListForSalonOwnerAsync(int ownerId, int salonId);
        Task<List<AvaliableSlotDTO>> GetAvailableSlotsAsync(int artistId, int[] serviceIds, DateTime date);
        Task<AppointmentsWithoutIdDTO> CreateAsync(AppointmentsWithoutIdDTO dto);

        /// <summary>Cancels an appointment for the current guest. Returns false if not found.</summary>
        Task<bool> CancelForGuestAsync(int guestId, int appointmentId);
    }
}
