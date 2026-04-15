using System.Collections.Generic;

namespace Salonic.BLL.DTOs.ServiceDTOs
{
    public class ServiceCsvImportResponseDTO
    {
        public List<ServiceWithIdDTO> Created { get; set; } = new();
        public List<ServiceCsvImportErrorDTO> Errors { get; set; } = new();
    }
}
