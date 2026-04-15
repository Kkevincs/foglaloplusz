namespace Salonic.BLL.DTOs.ServiceDTOs
{
    public class ServiceCsvImportErrorDTO
    {
        public int Line { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
