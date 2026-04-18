using Foglaloplusz.BLL.DTOs.AppointmentDTOs;
using Foglaloplusz.BLL.DTOs.ServiceDTOs;

namespace Foglaloplusz.BLL.Services.Interfaces
{
    public interface IServiceService
    {
        /// <summary>Vendég / nyilvános: csak Offeringhez kötött szolgáltatások.</summary>
        Task<List<ServiceWithIdDTO>> ListForSalonAsync(int salonId);
        /// <summary>Tulaj: teljes szalon-katalógus (minden Service sor).</summary>
        Task<List<ServiceWithIdDTO>> ListCatalogForSalonOwnerAsync(int ownerId, int salonId);
        /// <summary>Szakember: teljes szalon-katalógus, ha a szalon egyezik.</summary>
        Task<List<ServiceWithIdDTO>> ListCatalogForSalonArtistAsync(int artistId, int salonId);
        /// <summary>Guest / booking: services this artist offers in the salon (via Offerings).</summary>
        Task<List<ServiceWithIdDTO>> ListOfferedForArtistInSalonAsync(int salonId, int artistId);
        Task<List<ServiceWithIdDTO>> ListForCurrentArtistAsync(int artistId);
        Task<BookingNamesResponseDTO> GetBookingDisplayNamesAsync(BookingNamesRequestDTO? request);
        Task<ServiceWithIdDTO?> GetByIdForArtistAsync(int artistId, int salonId, int serviceId);
        Task<ServiceWithIdDTO> CreateForCurrentArtistAsync(int artistId, ServiceWithoutIdDTO? dto);
        Task<ServiceWithIdDTO> UpdateForCurrentArtistAsync(int artistId, int serviceId, ServiceWithoutIdDTO? dto);
        Task<ServiceCsvImportResponseDTO> ImportServicesFromCsvAsync(int artistId, string csv);
        Task<ServiceCsvImportResponseDTO> ImportSalonOwnerServicesFromCsvAsync(int ownerId, int salonId, string csv);

        Task<ServiceWithIdDTO> CreateForSalonOwnerAsync(int ownerId, int salonId, ServiceWithoutIdDTO? dto);
        Task<ServiceWithIdDTO> UpdateForSalonOwnerAsync(int ownerId, int salonId, int serviceId, ServiceWithoutIdDTO? dto);
        Task<bool> DeleteForSalonOwnerAsync(int ownerId, int salonId, int serviceId);

        Task<ServiceWithIdDTO> LinkSalonServiceForArtistAsync(int artistId, int serviceId);
        Task<bool> UnlinkOfferingForArtistAsync(int artistId, int offeringId);
    }
}
