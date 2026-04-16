using Foglaloplusz.BLL.DTOs.ServiceDTOs;

namespace Foglaloplusz.BLL.DTOs.AppointmentDTOs
{
    public class BookingNamesResponseDTO
    {
        public string ArtistName { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public List<ServiceIdNameDTO> Services { get; set; } = new();
    }
}
