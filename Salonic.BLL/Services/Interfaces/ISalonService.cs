using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.BLL.DTOs.SalonDTOs;
using Salonic.BLL.DTOs.ServiceDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Services.Interfaces
{
    public interface ISalonService
    {
        public Task<List<SalonWithIdDTO>> GetAll();
        public Task<SalonWithIdDTO> GetById(int id);
        public Task<SalonWithoutIdDTO> Create(SalonWithoutIdDTO artist);
        public Task Update(SalonWithIdDTO artist);
        public Task DeleteById(int id);
    }
}
