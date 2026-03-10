using Microsoft.EntityFrameworkCore;
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
    public class ServiceRepository : IServiceRepository
    {

        private readonly AppDbContext _context;

        public ServiceRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Service> Create(Service service)
        {
            _context.Add(service);
            _context.SaveChanges();
            return service;
        }

        public async Task DeleteById(int id)
        {
            var service = await _context.Services.Where(x => x.Id == id).Select(x => x).FirstAsync();
            _context.Remove(service);
            _context.SaveChanges();
        }

        public async Task<List<Service>> GetAll()
        {
            return await _context.Services.Select(x => x).ToListAsync();
        }

        public async Task<Service> GetById(int id)
        {
            return await _context.Services.Where(x => x.Id == id).Select(x => x).FirstAsync();
        }

        public async Task Update(Service service)
        {
            _context.Services.Update(service);
            _context.SaveChanges();
        }
    }
}
