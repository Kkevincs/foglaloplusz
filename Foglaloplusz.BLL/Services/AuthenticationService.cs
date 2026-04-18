using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Foglaloplusz.BLL.DTOs.UserDTOs;
using Foglaloplusz.BLL.Exceptions;
using Foglaloplusz.BLL.Services.Interfaces;
using Foglaloplusz.DAL.Repositories.Interfaces;

namespace Foglaloplusz.BLL.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAuthenticationRepository _auth;
        private readonly IConfiguration _configuration;

        public AuthenticationService(IAuthenticationRepository auth, IConfiguration configuration)
        {
            _auth = auth;
            _configuration = configuration;
        }

        public async Task<string?> SignInAsync(UserDTO? user)
        {
            EnsureValidLoginRequest(user);
            var role = user!.Role.Trim().ToLowerInvariant();
            int id;
            string email;

            switch (role)
            {
                case "guest":
                    var guest = await _auth.FindGuestByCredentials(user.Email, user.Pwd);
                    if (guest == null) return null;
                    id = guest.Id;
                    email = guest.Email;
                    break;
                case "artist":
                    if (!user.SalonId.HasValue || user.SalonId.Value <= 0)
                        throw new AppValidationException("A szalon azonosítója kötelező a szakember bejelentkezéshez.");
                    var artist = await _auth.FindArtistByCredentials(user.Email, user.Pwd);
                    if (artist == null) return null;
                    if (artist.SalonId != user.SalonId.Value)
                        throw new AppValidationException("A szakember nem ehhez a szalonhoz tartozik.");
                    id = artist.Id;
                    email = artist.Email;
                    break;
                case "owner":
                    var owner = await _auth.FindOwnerByCredentials(user.Email, user.Pwd);
                    if (owner == null) return null;
                    id = owner.Id;
                    email = owner.Email;
                    break;
                default:
                    return null;
            }

            return CreateJwt(id, email, role);
        }

        public Task<bool> EmailExistsAsync(string email) =>
            _auth.EmailExists(email ?? string.Empty);

        public void EnsureValidLoginRequest(UserDTO? authInfo)
        {
            if (authInfo == null)
                throw new AppValidationException("A kérés törzse kötelező.");
            if (string.IsNullOrWhiteSpace(authInfo.Email) ||
                authInfo.Pwd == null ||
                string.IsNullOrWhiteSpace(authInfo.Role))
                throw new AppValidationException("Az e-mail, a jelszó és a szerepkör kötelező.");
            var role = authInfo.Role.Trim().ToLowerInvariant();
            if (role is not ("guest" or "artist" or "owner"))
                throw new AppValidationException("A szerepkör csak guest, artist vagy owner lehet.");
        }

        private string CreateJwt(int id, string email, string role)
        {
            var secret = _configuration["Authentication:SecretForKey"]
                ?? throw new InvalidOperationException("Authentication:SecretForKey is not configured.");
            var key = new SymmetricSecurityKey(Convert.FromBase64String(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new("sub", id.ToString()),
                new("email", email),
                new("role", role)
            };

            var token = new JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claims,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
