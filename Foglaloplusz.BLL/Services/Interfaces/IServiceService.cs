using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
using Foglaloplusz.BLL.DTOs.ServiceDTOs;

namespace Foglaloplusz.BLL.Services.Interfaces
{
    public interface IServiceService
    {
        Task<List<ServiceWithIdDTO>> ListForSalonAsync(int salonId);
        Task<List<ServiceWithIdDTO>> ListForCurrentArtistAsync(int artistId);
        Task<BookingNamesResponseDTO> GetBookingDisplayNamesAsync(BookingNamesRequestDTO request);
        Task<ServiceWithIdDTO?> GetByIdForArtistAsync(int artistId, int salonId, int serviceId);
        Task<ServiceWithIdDTO> CreateForCurrentArtistAsync(int artistId, ServiceWithoutIdDTO dto);

        Task<ServiceCsvImportResponseDTO> ImportServicesFromCsvAsync(int artistId, string csv);
    }
}
