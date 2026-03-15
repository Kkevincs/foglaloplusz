using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.BLL.DTOs.OwnerDTOs;
using Salonic.BLL.DTOs.ServiceDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Services.Interfaces
{
    public interface IOwnerService
    {
        public Task<List<OwnerWithIdDTO>> GetAll();
        public Task<OwnerWithIdDTO> GetById(int id);
        public Task<OwnerWithoutIdDTO> Create(OwnerWithoutIdDTO artist);
        public Task Update(OwnerWithIdDTO artist);
        public Task DeleteById(int id);
    }
}
