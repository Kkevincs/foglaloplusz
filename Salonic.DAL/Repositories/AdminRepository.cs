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
    public class AdminRepository : IAdminRepository
    {

        private readonly AppDbContext _context;

        public AdminRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Admin> Create(Admin admin)
        {
            _context.Admins.Add(admin);
            _context.SaveChanges();
            return admin;
        }

        public async Task DeleteById(int id)
        {
            Admin admin =  _context.Admins.Where(x => x.Id == id).FirstOrDefault();
            _context.Remove(admin);
            _context.SaveChanges();
        }

        public async Task<List<Admin>> GetAll()
        {
            return _context.Admins.Select(x => x).ToList();
        }

        public async Task<Admin> GetById(int id)
        {
            return _context.Admins.Where(x => x.Id == id).Select(x => x).FirstOrDefault();
        }

        public async Task Update(Admin admin)
        {
            _context.Admins.Update(admin);
            await _context.SaveChangesAsync();
        }
    }
}
