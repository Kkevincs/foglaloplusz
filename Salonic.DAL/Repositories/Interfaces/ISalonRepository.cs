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
        public Task<List<Salon>> GetAll();
        public Task<Salon> GetById(int id);
        public Task<Salon> Create(Salon salon);
        public Task Update(Salon salon);
        public Task DeleteById(int id);
    }
}
