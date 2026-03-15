using Salonic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.DAL.Repositories.Interfaces
{
    public interface IAdminRepository
    {
        public Task<List<Admin>> GetAll();
        public Task<Admin> GetById(int id);
        public Task<Admin> Create(Admin admin);
        public Task Update(Admin admin);
        public Task DeleteById(int id);
    }
}
