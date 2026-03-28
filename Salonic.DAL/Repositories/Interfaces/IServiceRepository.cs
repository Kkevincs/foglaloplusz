using Salonic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.DAL.Repositories.Interfaces
{
    public interface IServiceRepository
    {
        public Task<List<Service>> GetAll();
        public Task<Service?> GetById(int id);
        public Task<Service[]> GetByIds(int[] ids);
        public Task<Service> Create(Service service);
        public Task Update(Service service);
        public Task DeleteById(int id);

    }
}
