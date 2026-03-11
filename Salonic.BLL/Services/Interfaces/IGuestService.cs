using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.BLL.DTOs.GuestDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Services.Interfaces
{
    public interface IGuestService
    {
        public Task<List<GuestWithIdDTO>> GetAll();
        public Task<GuestWithIdDTO> GetById(int id);
        public Task<GuestWithoutIdDTO> Create(GuestWithoutIdDTO guest);
        public Task Update(GuestWithIdDTO guest);
        public Task DeleteById(int id);
    }
}
