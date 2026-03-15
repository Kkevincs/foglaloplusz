using AutoMapper;
using Salonic.BLL.DTOs.OwnerDTOs;
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
    public class OwnerService : IOwnerService
    {
        private readonly IOwnerRepository _ownerRepository;
        private readonly IMapper _mapper;

        public OwnerService(IOwnerRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _ownerRepository = repo;
        }

        public async Task<OwnerWithoutIdDTO> Create(OwnerWithoutIdDTO ownerDto)
        {
            var owner = _mapper.Map<Owner>(ownerDto);
            await _ownerRepository.Create(owner);
            return ownerDto;
        }

        public async Task DeleteById(int id)
        {
            await _ownerRepository.DeleteById(id);
        }

        public async Task<List<OwnerWithIdDTO>> GetAll()
        {
            var owners = await _ownerRepository.GetAll();
            var ownerDtos = _mapper.Map<List<OwnerWithIdDTO>>(owners);
            return ownerDtos;
        }

        public async Task<OwnerWithIdDTO> GetById(int id)
        {
            var owner = await _ownerRepository.GetById(id);
            var ownerDto = _mapper.Map<OwnerWithIdDTO>(owner);
            return ownerDto;
        }

        public async Task Update(OwnerWithIdDTO ownerDto)
        {
            var owner = _mapper.Map<Owner>(ownerDto);
            await _ownerRepository.Update(owner);
        }
    }
}
