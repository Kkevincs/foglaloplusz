using Salonic.DAL.Data;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.DAL.Repositories
{
    public class OwnerRepository : IOwnerRepository
    {

        private readonly AppDbContext _context;

        public OwnerRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Owner> Create(Owner owner)
        {
            _context.Owners.Add(owner);
            _context.SaveChanges();
            return owner;
        }

        public async Task DeleteById(int id)
        {
            Owner owner =  _context.Owners.Where(x => x.Id == id).FirstOrDefault();
            _context.Remove(owner);
            _context.SaveChanges();
        }

        public async Task<List<Owner>> GetAll()
        {
            return _context.Owners.Select(x => x).ToList();
        }

        public async Task<Owner> GetById(int id)
        {
            return _context.Owners.Where(x => x.Id == id).Select(x => x).FirstOrDefault();
        }

        public async Task Update(Owner owner)
        {
            _context.Owners.Update(owner);
            await _context.SaveChangesAsync();
        }
    }
}
