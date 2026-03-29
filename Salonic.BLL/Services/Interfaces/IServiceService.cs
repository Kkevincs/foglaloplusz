using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.BLL.DTOs.ServiceDTOs;

namespace Salonic.BLL.Services.Interfaces
{
    public interface IServiceService
    {
        Task<List<ServiceWithIdDTO>> ListForSalonAsync(int salonId);

        Task<BookingNamesResponseDTO> GetBookingDisplayNamesAsync(BookingNamesRequestDTO request);

        Task<ServiceWithIdDTO?> GetByIdForArtistAsync(int artistId, int salonId, int serviceId);
        Task<ServiceWithIdDTO> CreateForArtistAsync(int artistId, int salonId, ServiceWithoutIdDTO dto);
        Task UpdateForArtistAsync(int artistId, int salonId, ServiceWithIdDTO dto);
        Task DeleteForArtistAsync(int artistId, int salonId, int serviceId);

        Task<ServiceWithIdDTO> CreateForCurrentArtistAsync(int artistId, ServiceWithoutIdDTO dto);
        Task UpdateForCurrentArtistAsync(int artistId, ServiceWithIdDTO dto);
        Task DeleteForCurrentArtistAsync(int artistId, int serviceId);
    }
}
