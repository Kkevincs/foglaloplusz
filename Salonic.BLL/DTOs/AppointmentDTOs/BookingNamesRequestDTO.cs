namespace Salonic.BLL.DTOs.AppointmentDTOs
{
    public class BookingNamesRequestDTO
    {
        public int ArtistId { get; set; }
        public int GuestId { get; set; }
        public List<int> ServiceIds { get; set; } = new();
    }
}
