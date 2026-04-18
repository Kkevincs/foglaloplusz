using AutoMapper;
using Foglaloplusz.BLL.DTOs.OwnerDTOs;
using Foglaloplusz.BLL.DTOs.UserDTOs;
using Foglaloplusz.BLL.Exceptions;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.DAL.Models;
using Foglaloplusz.DAL.Repositories.Interfaces;

namespace Foglaloplusz.BLL.Services
{
    public class OwnerService : IOwnerService
    {
        private readonly IOwnerRepository _ownerRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMapper _mapper;

        public OwnerService(IOwnerRepository repo, IAuthenticationService authenticationService, IMapper mapper)
        {
            _ownerRepository = repo;
            _authenticationService = authenticationService;
            _mapper = mapper;
        }

        public async Task<OwnerWithoutIdDTO> RegisterAsync(OwnerWithoutIdDTO? ownerDto)
        {
            if (ownerDto == null)
                throw new AppValidationException("A kérés törzse kötelező.");
            if (await _authenticationService.EmailExistsAsync(ownerDto.Email))
                throw new AppValidationException("Ez az e-mail cím már regisztrálva van.");
            return await CreateAsync(ownerDto);
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

        public async Task<OwnerWithIdDTO> GetMeOrThrowAsync(int ownerId)
        {
            var owner = await GetByIdAsync(ownerId);
            return owner ?? throw new AppNotFoundException("A tulajdonos profilja nem található.");
        }

        public async Task<OwnerWithIdDTO> UpdateProfileAsync(int ownerId, UserProfileUpdateDTO dto)
        {
            if (dto == null)
                throw new AppValidationException("A kérés törzse kötelező.");
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new AppValidationException("A név kötelező.");
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new AppValidationException("Az e-mail kötelező.");

            var entity = await _ownerRepository.GetByIdAsync(ownerId);
            if (entity == null)
                throw new AppNotFoundException("A tulajdonos profilja nem található.");

            var newEmail = dto.Email.Trim();
            if (!string.Equals(newEmail, entity.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _authenticationService.EmailExistsAsync(newEmail))
                    throw new AppValidationException("Ez az e-mail cím már regisztrálva van.");
            }

            ApplyOwnerPasswordChange(entity, dto);

            entity.Name = dto.Name.Trim();
            entity.Email = newEmail;
            await _ownerRepository.UpdateAsync(entity);
            return _mapper.Map<OwnerWithIdDTO>(entity);
        }

        private static void ApplyOwnerPasswordChange(Owner entity, UserProfileUpdateDTO dto)
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
