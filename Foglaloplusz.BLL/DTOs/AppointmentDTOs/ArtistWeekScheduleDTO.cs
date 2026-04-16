namespace Foglaloplusz.BLL.DTOs.AppointmentDTOs
{
    public class ArtistWeekScheduleDTO
    {
        public string WeekStart { get; set; } = string.Empty;
        public int ArtistId { get; set; }
        public IList<int> Hours { get; set; } = new List<int>();
        public IList<ArtistWeekScheduleDayDTO> Days { get; set; } = new List<ArtistWeekScheduleDayDTO>();
    }

    public class ArtistWeekScheduleDayDTO
    {
        public string Date { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public IList<ArtistWeekScheduleCellDTO> Cells { get; set; } = new List<ArtistWeekScheduleCellDTO>();
    }

    public class ArtistWeekScheduleCellDTO
    {
        public int Hour { get; set; }
        /// <summary>open, booked, or closed</summary>
        public string Status { get; set; } = string.Empty;
        public string? Detail { get; set; }
    }
}
