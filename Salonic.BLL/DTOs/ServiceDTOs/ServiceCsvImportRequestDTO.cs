namespace Salonic.BLL.DTOs.ServiceDTOs
{
    public class ServiceCsvImportRequestDTO
    {
        /// <summary>Raw CSV text: servicename;duration;description;price per line.</summary>
        public string CsvText { get; set; } = string.Empty;
    }
}
