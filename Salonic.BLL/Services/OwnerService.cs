using AutoMapper;
using Salonic.BLL.DTOs.OwnerDTOs;
using Salonic.BLL.Services.Interfaces;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories.Interfaces;

namespace Salonic.BLL.Services
{
    public class OwnerService : IOwnerService
    {
        private readonly IOwnerRepository _ownerRepository;
        private readonly IMapper _mapper;

        public OwnerService(IOwnerRepository repo, IMapper mapper)
        {
            _ownerRepository = repo;
            _mapper = mapper;
        }

        public async Task<OwnerWithoutIdDTO> CreateAsync(OwnerWithoutIdDTO ownerDto)
        {
            var owner = _mapper.Map<Owner>(ownerDto);
            await _ownerRepository.AddAsync(owner);
            return ownerDto;
        }

        public async Task<OwnerWithIdDTO?> GetByIdAsync(int id)
        {
            var owner = await _ownerRepository.GetByIdAsync(id);
            return owner == null ? null : _mapper.Map<OwnerWithIdDTO>(owner);
        }
    }
}
