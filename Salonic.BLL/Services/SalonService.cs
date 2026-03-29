using AutoMapper;
using Salonic.BLL.DTOs.SalonDTOs;
using Salonic.BLL.Services.Interfaces;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories.Interfaces;

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

        public async Task<SalonWithIdDTO> CreateForOwner(int ownerId, SalonWithoutIdDTO salon)
        {
            if (ownerId <= 0)
                throw new ArgumentException("Owner id must be positive.", nameof(ownerId));

            var entity = _mapper.Map<Salon>(salon);
            entity.Id = 0;
            entity.Artists = new List<Artist>();
            entity.Appointments = new List<Appointment>();

            var created = await _salonRepository.CreateForOwner(ownerId, entity);
            return _mapper.Map<SalonWithIdDTO>(created);
        }

        public async Task DeleteForOwner(int ownerId, int salonId)
        {
            if (ownerId <= 0)
                throw new ArgumentException("Owner id must be positive.", nameof(ownerId));
            if (salonId <= 0)
                throw new ArgumentException("Salon id must be positive.", nameof(salonId));

            var deleted = await _salonRepository.DeleteForOwner(ownerId, salonId);
            if (!deleted)
                throw new InvalidOperationException("Salon was not found.");
        }

        public async Task<List<SalonWithIdDTO>> GetAllForOwner(int ownerId)
        {
            if (ownerId <= 0)
                throw new ArgumentException("Owner id must be positive.", nameof(ownerId));

            var salons = await _salonRepository.GetAllByOwnerId(ownerId);
            return _mapper.Map<List<SalonWithIdDTO>>(salons);
        }

        public async Task<SalonWithIdDTO?> GetByIdForOwner(int ownerId, int salonId)
        {
            if (ownerId <= 0)
                throw new ArgumentException("Owner id must be positive.", nameof(ownerId));
            if (salonId <= 0)
                return null;

            var salon = await _salonRepository.GetByOwnerAndId(ownerId, salonId);
            return salon == null ? null : _mapper.Map<SalonWithIdDTO>(salon);
        }

        public async Task UpdateForOwner(int ownerId, SalonWithIdDTO salon)
        {
            if (ownerId <= 0)
                throw new ArgumentException("Owner id must be positive.", nameof(ownerId));
            if (salon.Id <= 0)
                throw new ArgumentException("Salon id must be set.", nameof(salon));

            var entity = _mapper.Map<Salon>(salon);
            await _salonRepository.UpdateForOwner(ownerId, entity);
        }
    }
}
