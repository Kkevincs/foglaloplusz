namespace Foglaloplusz.BLL.DTOs.SalonDTOs
{
    /// <summary>Public salon listing for the portal (no owner e-mail).</summary>
    public class SalonDirectoryCardDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Theme { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string? Location { get; set; }
    }
}
