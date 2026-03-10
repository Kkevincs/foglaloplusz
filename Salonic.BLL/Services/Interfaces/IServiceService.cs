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
        public Task<List<GetService>> GetAll();
        public Task<GetService> GetById(int id);
        public Task<GetService> Create(GetService service);
        public Task Update(GetService service);
        public Task DeleteById(int id);
    }
}
