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
        public Task<List<GetGuest>> GetAll();
        public Task<GetGuest> GetById(int id);
        public Task<AddGuest> Create(AddGuest guest);
        public Task Update(GetGuest guest);
        public Task DeleteById(int id);
    }
}
