using Salonic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.DAL.Repositories.Interfaces
{
    public interface IOwnerRepository
    {
        public Task<List<Owner>> GetAll();
        public Task<Owner> GetById(int id);
        public Task<Owner> Create(Owner owner);
        public Task Update(Owner owner);
        public Task DeleteById(int id);
    }
}
