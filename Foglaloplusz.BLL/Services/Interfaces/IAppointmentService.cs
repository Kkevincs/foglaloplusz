using Foglaloplusz.BLL.DTOs.AppointmentDTOs;

namespace Foglaloplusz.BLL.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<List<AppointmentsWithIdDTO>> ListForGuestAsync(int guestId);
        Task<List<AppointmentsWithIdDTO>> ListForSalonOwnerAsync(int ownerId, int salonId);
        Task<List<AvaliableSlotDTO>> GetAvailableSlotsAsync(int artistId, int[] serviceIds, DateTime date);
        Task<AppointmentsWithoutIdDTO> CreateAsync(AppointmentsWithoutIdDTO dto);
    }
}
