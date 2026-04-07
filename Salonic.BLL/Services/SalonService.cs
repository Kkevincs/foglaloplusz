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

        public Task<string?> GetThemeBySalonIdAsync(int salonId) =>
            salonId <= 0 ? Task.FromResult<string?>(null) : _salonRepository.GetThemeBySalonIdAsync(salonId);

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
