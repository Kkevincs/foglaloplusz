using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.BLL.DTOs.ServiceDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Services.Interfaces
{
    public interface IServiceService
    {
        public Task<List<ServiceWithIdDTO>> GetAll();
        public Task<ServiceWithIdDTO> GetById(int id);
        public Task<ServiceWithIdDTO> Create(ServiceWithIdDTO service);
        public Task Update(ServiceWithIdDTO service);
        public Task DeleteById(int id);
    }
}
