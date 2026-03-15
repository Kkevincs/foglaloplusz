using Salonic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.DAL.Repositories.Interfaces
{
    public interface ISalonRepository
    {
        public Task<List<Guest>> GetAll();
        public Task<Guest> GetById(int id);
        public Task<Guest> Create(Guest guest);
        public Task Update(Guest guest);
        public Task DeleteById(int id);
    }
}
