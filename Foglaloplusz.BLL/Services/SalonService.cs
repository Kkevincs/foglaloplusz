using AutoMapper;
using Foglaloplusz.BLL.DTOs.SalonDTOs;
using Foglaloplusz.BLL.Exceptions;
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
                throw new AppValidationException("A tulajdonos azonosítója érvénytelen.");

            var entity = _mapper.Map<Salon>(salon);
            entity.OwnerId = ownerId;
            entity.Artists = new List<Artist>();
            entity.Appointments = new List<Appointment>();

            await _salonRepository.AddAsync(entity);
            return _mapper.Map<SalonWithIdDTO>(entity);
        }

        public async Task<SalonWithIdDTO> UpdateForOwnerAsync(
            int ownerId,
            int salonId,
            SalonWithoutIdDTO dto,
            string? newLogoRelativePath,
            CancellationToken cancellationToken = default)
        {
            if (ownerId <= 0)
                throw new AppValidationException("A tulajdonos azonosítója érvénytelen.");
            if (salonId <= 0)
                throw new AppValidationException("A szalon azonosítója érvénytelen.");
            if (dto == null)
                throw new AppValidationException("A kérés törzse kötelező.");

            var existing = await _salonRepository.GetByOwnerAndIdAsync(ownerId, salonId);
            if (existing == null)
                throw new AppNotFoundException("A szalon nem található, vagy nincs jogosultságod hozzá.");

            var oldLogoUrl = existing.LogoUrl;

            existing.Name = dto.Name.Trim();
            existing.Email = dto.Email.Trim();
            existing.Theme = dto.Theme?.Trim() ?? string.Empty;
            existing.Description = dto.Description?.Trim() ?? string.Empty;
            existing.Location = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location.Trim();

            if (!string.IsNullOrWhiteSpace(newLogoRelativePath))
            {
                existing.LogoUrl = newLogoRelativePath.Trim();
            }

            await _salonRepository.SaveChangesAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(newLogoRelativePath) &&
                !string.Equals(oldLogoUrl, existing.LogoUrl, StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(oldLogoUrl))
            {
                await _salonLogoCleanup.TryDeleteByRelativeUrlAsync(oldLogoUrl, cancellationToken);
            }

            return _mapper.Map<SalonWithIdDTO>(existing);
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
            var dto = _mapper.Map<SalonWithIdDTO>(entity);
            dto.OwnerName = entity.Owner?.Name;
            return dto;
        }

        public async Task<List<SalonWithIdDTO>> ListPublicAsync()
        {
            var salons = await _salonRepository.ListAllOrderedByNameAsync();
            return _mapper.Map<List<SalonWithIdDTO>>(salons);
        }

        public async Task<List<SalonWithIdDTO>> ListForOwnerAsync(int ownerId)
        {
            if (ownerId <= 0)
                throw new AppValidationException("A tulajdonos azonosítója érvénytelen.");

            var salons = await _salonRepository.ListByOwnerIdAsync(ownerId);
            return _mapper.Map<List<SalonWithIdDTO>>(salons);
        }

        public async Task DeleteForOwnerAsync(int ownerId, int salonId, CancellationToken cancellationToken = default)
        {
            if (ownerId <= 0)
                throw new AppValidationException("A tulajdonos azonosítója érvénytelen.");
            if (salonId <= 0)
                throw new AppValidationException("A szalon azonosítója érvénytelen.");

            var existing = await _salonRepository.GetByOwnerAndIdAsync(ownerId, salonId);
            if (existing == null)
                throw new AppNotFoundException("A szalon nem található ehhez a fiókhoz.");

            var logoUrl = existing.LogoUrl;

            var deleted = await _salonRepository.DeleteCascadeByOwnerAsync(ownerId, salonId, cancellationToken);
            if (!deleted)
                throw new AppNotFoundException("A szalon nem törölhető (lehet, hogy már nem létezik).");

            await _salonLogoCleanup.TryDeleteByRelativeUrlAsync(logoUrl, cancellationToken);
        }
    }
}
