using AutoMapper;
using Foglaloplusz.BLL.DTOs.SalonDTOs;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.DAL.Models;
using Foglaloplusz.DAL.Repositories.Interfaces;

namespace Foglaloplusz.BLL.Services
{
    public class SalonService : ISalonService
    {
        private readonly IMapper _mapper;
        private readonly ISalonRepository _salonRepository;
        private readonly ISalonLogoCleanup _salonLogoCleanup;

        public SalonService(ISalonRepository repo, IMapper mapper, ISalonLogoCleanup salonLogoCleanup)
        {
            _mapper = mapper;
            _salonRepository = repo;
            _salonLogoCleanup = salonLogoCleanup;
        }

        public async Task<SalonWithIdDTO> CreateForOwnerAsync(int ownerId, SalonWithoutIdDTO salon)
        {
            if (ownerId <= 0)
                throw new ArgumentException("Owner id must be positive.", nameof(ownerId));

            var entity = _mapper.Map<Salon>(salon);
            entity.OwnerId = ownerId;
            entity.Artists = new List<Artist>();
            entity.Appointments = new List<Appointment>();

            await _salonRepository.AddAsync(entity);
            return _mapper.Map<SalonWithIdDTO>(entity);
        }

        public Task<bool> ExistsAsync(int salonId) =>
            salonId <= 0 ? Task.FromResult(false) : _salonRepository.ExistsByIdAsync(salonId);

        public async Task<List<SalonDirectoryCardDTO>> ListPublicDirectoryAsync()
        {
            var salons = await _salonRepository.ListAllOrderedByNameAsync();
            return _mapper.Map<List<SalonDirectoryCardDTO>>(salons);
        }

        public async Task<SalonWithIdDTO?> GetPublicProfileAsync(int salonId)
        {
            if (salonId <= 0) return null;
            var entity = await _salonRepository.GetBySalonIdAsync(salonId);
            if (entity == null) return null;
            return _mapper.Map<SalonWithIdDTO>(entity);
        }

        public async Task<List<SalonWithIdDTO>> ListPublicAsync()
        {
            var salons = await _salonRepository.ListAllAsync();
            return _mapper.Map<List<SalonWithIdDTO>>(salons);
        }

        public async Task<List<SalonWithIdDTO>> ListForOwnerAsync(int ownerId)
        {
            if (ownerId <= 0)
                throw new ArgumentException("Owner id must be positive.", nameof(ownerId));

            var salons = await _salonRepository.ListByOwnerIdAsync(ownerId);
            return _mapper.Map<List<SalonWithIdDTO>>(salons);
        }

        public async Task DeleteForOwnerAsync(int ownerId, int salonId, CancellationToken cancellationToken = default)
        {
            if (ownerId <= 0)
                throw new ArgumentException("Owner id must be positive.", nameof(ownerId));
            if (salonId <= 0)
                throw new ArgumentException("Salon id must be positive.", nameof(salonId));

            var existing = await _salonRepository.GetByOwnerAndIdAsync(ownerId, salonId);
            if (existing == null)
                throw new InvalidOperationException("Salon was not found for this owner.");

            var logoUrl = existing.LogoUrl;

            var deleted = await _salonRepository.DeleteCascadeByOwnerAsync(ownerId, salonId, cancellationToken);
            if (!deleted)
                throw new InvalidOperationException("Salon could not be deleted (it may have already been removed).");

            await _salonLogoCleanup.TryDeleteByRelativeUrlAsync(logoUrl, cancellationToken);
        }
    }
}
