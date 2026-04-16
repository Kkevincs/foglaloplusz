using AutoMapper;
using Foglaloplusz.BLL.DTOs.GuestDTOs;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.DAL.Models;
using Foglaloplusz.DAL.Repositories.Interfaces;

namespace Foglaloplusz.BLL.Services
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
