using AutoMapper;
using Salonic.BLL.DTOs.GuestDTOs;
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
        private readonly IGuestRepository _guestRepository;
        private readonly IMapper _mapper;

        public AdminService(IAdminRepository repo, IMapper mapper) { 
            _mapper = mapper;
            _adminRepository = repo;
        }

        public async Task<GuestWithoutIdDTO> Create(GuestWithoutIdDTO guest)
        {
            var Guest = _mapper.Map<Guest>(guest);
            Guest newGuest = await _guestRepository.Create(Guest);
            return guest;
        }

        public async Task DeleteById(int id) { 
            await _guestRepository.DeleteById(id);
        }

        public async Task<List<GuestWithIdDTO>> GetAll() { 
            var guests = await _guestRepository.GetAll();
            var guestDTO = _mapper.Map<List<GuestWithIdDTO>>(guests);
            return guestDTO;
        }

        public async Task<GuestWithIdDTO> GetById(int id) { 
            var guest = await _guestRepository.GetById(id);
            var guestDTO = _mapper.Map<GuestWithIdDTO>(guest);
            return guestDTO;
        }

        public async Task Update(GuestWithIdDTO guest) {
            var oldGuest = _mapper.Map<Guest>(guest);
            await _guestRepository.Update(oldGuest);
        }
    }
}
