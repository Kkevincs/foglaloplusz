using AutoMapper;
using Salonic.BLL.DTOs.ArtistDTOs;
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
    public class ArtistService : IArtistService
    {
        private readonly IMapper _mapper;
        private readonly IArtistRepository _artistRepository;
        private readonly ISalonRepository _salonRepository;

        public ArtistService(IArtistRepository repo, ISalonRepository salonRepository, IMapper mapper) {
            _mapper = mapper;
            _artistRepository = repo;
            _salonRepository = salonRepository;
        }

        public async Task<ArtistWithoutIdDTO> Create(ArtistWithoutIdDTO artist) {
            var newArtist = _mapper.Map<Artist>(artist);
            await _artistRepository.Create(newArtist);
            return artist;
        }

        public async Task DeleteById(int id) { 
           await _artistRepository.DeleteById(id);
        }

        public async Task<List<ArtistWithIdDTO>> GetAll() { 
            var artists = await _artistRepository.GetAll();
            var ArtistsDTO = _mapper.Map<List<ArtistWithIdDTO>>(artists);
            return ArtistsDTO;
        }

        public async Task<List<ArtistWithIdDTO>?> GetBySalonIdForOwner(int ownerId, int salonId)
        {
            if (ownerId <= 0 || salonId <= 0)
                return null;

            var salon = await _salonRepository.GetByOwnerAndId(ownerId, salonId);
            if (salon == null)
                return null;

            var artists = await _artistRepository.GetBySalonId(salonId);
            return _mapper.Map<List<ArtistWithIdDTO>>(artists);
        }

        public async Task<ArtistWithIdDTO> GetById(int id) { 
            var artist = await _artistRepository.GetById(id);
            var artistDTO = _mapper.Map<ArtistWithIdDTO>(artist);
            return artistDTO;
        }

        public async Task Update(ArtistWithIdDTO artist) {
            var artistDTO = _mapper.Map<Artist>(artist);
            var newArtist = _artistRepository.Update(artistDTO);
        }
    }
}
