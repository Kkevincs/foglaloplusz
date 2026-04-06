using AutoMapper;
using Salonic.BLL.DTOs.GuestDTOs;
using Salonic.BLL.Services.Interfaces;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories.Interfaces;

namespace Salonic.BLL.Services
{
    public class GuestService : IGuestService
    {
        private readonly IGuestRepository _guestRepository;
        private readonly IMapper _mapper;

        public GuestService(IGuestRepository repo, IMapper mapper)
        {
            _guestRepository = repo;
            _mapper = mapper;
        }

        public async Task<GuestWithoutIdDTO> CreateAsync(GuestWithoutIdDTO guest)
        {
            var entity = _mapper.Map<Guest>(guest);
            await _guestRepository.AddAsync(entity);
            return guest;
        }

        public async Task<GuestWithIdDTO?> GetByIdAsync(int id)
        {
            var guest = await _guestRepository.GetByIdAsync(id);
            return guest == null ? null : _mapper.Map<GuestWithIdDTO>(guest);
        }
    }
}
