using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Salonic.BLL.DTOs.AdminDTOs;

namespace Salonic.BLL.Services.Interfaces
{
    public interface IAdminService
    {
        public Task<List<AdminWithIdDTO>> GetAll();
        public Task<AdminWithIdDTO> GetById(int id);
        public Task<AdminWithoutIdDTO> Create(AdminWithoutIdDTO artist);
        public Task Update(AdminWithIdDTO artist);
        public Task DeleteById(int id);
    }
}
