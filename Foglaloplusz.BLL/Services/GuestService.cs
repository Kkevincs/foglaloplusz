using AutoMapper;
using Foglaloplusz.BLL.DTOs.GuestDTOs;
using Foglaloplusz.BLL.DTOs.UserDTOs;
using Foglaloplusz.BLL.Exceptions;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.DAL.Models;
using Foglaloplusz.DAL.Repositories.Interfaces;

namespace Foglaloplusz.BLL.Services
{
    public class GuestService : IGuestService
    {
        private readonly IGuestRepository _guestRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMapper _mapper;

        public GuestService(
            IGuestRepository repo,
            IAuthenticationService authenticationService,
            IMapper mapper)
        {
            _guestRepository = repo;
            _authenticationService = authenticationService;
            _mapper = mapper;
        }

        public async Task<GuestWithoutIdDTO> RegisterAsync(GuestWithoutIdDTO? guest)
        {
            if (guest == null)
                throw new AppValidationException("A kérés törzse kötelező.");
            if (await _authenticationService.EmailExistsAsync(guest.Email))
                throw new AppValidationException("Ez az e-mail cím már regisztrálva van.");
            return await CreateAsync(guest);
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

        public async Task<GuestWithIdDTO> GetMeOrThrowAsync(int guestId, bool callerIsGuestRole)
        {
            var g = await GetByIdAsync(guestId);
            if (g != null)
                return g;
            if (callerIsGuestRole)
                throw new AppValidationException("A vendégprofil nem található.");
            throw new AppForbiddenException("Nincs jogosultságod a profil megtekintéséhez.");
        }

        public async Task<GuestWithIdDTO> UpdateProfileAsync(int guestId, UserProfileUpdateDTO dto)
        {
            if (dto == null)
                throw new AppValidationException("A kérés törzse kötelező.");
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new AppValidationException("A név kötelező.");
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new AppValidationException("Az e-mail kötelező.");

            var entity = await _guestRepository.GetByIdAsync(guestId);
            if (entity == null)
                throw new AppNotFoundException("A vendégprofil nem található.");

            var newEmail = dto.Email.Trim();
            if (!string.Equals(newEmail, entity.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _authenticationService.EmailExistsAsync(newEmail))
                    throw new AppValidationException("Ez az e-mail cím már regisztrálva van.");
            }

            ApplyPasswordChange(entity, dto);

            entity.Name = dto.Name.Trim();
            entity.Email = newEmail;
            await _guestRepository.UpdateAsync(entity);
            return _mapper.Map<GuestWithIdDTO>(entity);
        }

        private static void ApplyPasswordChange(Guest entity, UserProfileUpdateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                return;
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || dto.CurrentPassword != entity.Pwd)
                throw new AppValidationException("A jelenlegi jelszó nem megfelelő.");
            if (dto.NewPassword.Length < 6)
                throw new AppValidationException("Az új jelszó legalább 6 karakter legyen.");
            entity.Pwd = dto.NewPassword;
        }
    }
}
