using AutoMapper;
using Salonic.BLL.DTOs.AdminDTOs;
using Salonic.BLL.DTOs.ServiceDTOs;
using Salonic.BLL.Services.Interfaces;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly IMapper _mapper;

        public AdminService(IAdminRepository repo, IMapper mapper) { 
            _mapper = mapper;
            _adminRepository = repo;
        }

        public async Task<AdminWithoutIdDTO> Create(AdminWithoutIdDTO admin)
        {
            var Admin = _mapper.Map<Admin>(admin);
            Admin newAdmin = await _adminRepository.Create(Admin);
            return admin;
        }

        public async Task DeleteById(int id) { 
            await _adminRepository.DeleteById(id);
        }

        public async Task<List<AdminWithIdDTO>> GetAll() { 
            var admins = await _adminRepository.GetAll();
            var adminDTO = _mapper.Map<List<AdminWithIdDTO>>(admins);
            return adminDTO;
        }

        public async Task<AdminWithIdDTO> GetById(int id) { 
            var admin = await _adminRepository.GetById(id);
            var adminDTO = _mapper.Map<AdminWithIdDTO>(admin);
            return adminDTO;
        }

        public async Task Update(AdminWithIdDTO admin) {
            var oldAdmin = _mapper.Map<Admin>(admin);
            await _adminRepository.Update(oldAdmin);
        }
    }
}
