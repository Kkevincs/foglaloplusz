namespace Salonic.BLL.DTOs.ServiceDTOs
{
    public class ServiceWithIdDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public int DurationInMinute { get; set; }

        public int CreatedByArtistId { get; set; }
    }
}
