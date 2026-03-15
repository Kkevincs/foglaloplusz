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
    public class SalonService : ISalonService
    {
        private readonly IMapper _mapper;
        private readonly ISalonRepository _salonRepository;

        public SalonService(ISalonRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _salonRepository = repo;
        }

        public async Task<SalonWithoutIdDTO> Create(SalonWithoutIdDTO salon)
        {
            var newSalon = _mapper.Map<Salon>(salon);
            await _salonRepository.Create(newSalon);
            return salon;
        }

        public async Task DeleteById(int id)
        {
            await _salonRepository.DeleteById(id);
        }

        public async Task<List<SalonWithIdDTO>> GetAll()
        {
            var salons = await _salonRepository.GetAll();
            var salonsDTO = _mapper.Map<List<SalonWithIdDTO>>(salons);
            return salonsDTO;
        }

        public async Task<SalonWithIdDTO> GetById(int id)
        {
            var salon = await _salonRepository.GetById(id);
            var salonDTO = _mapper.Map<SalonWithIdDTO>(salon);
            return salonDTO;
        }

        public async Task Update(SalonWithIdDTO salon)
        {
            var salonDTO = _mapper.Map<Salon>(salon);
            var newSalon = _salonRepository.Update(salonDTO);
        }
    }
}
