using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Foglaloplusz.BLL.DTOs.UserDTOs;
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

        public async Task<string?> SignInAsync(UserDTO user)
        {
            if (user == null ||
                string.IsNullOrWhiteSpace(user.Email) ||
                user.Pwd == null ||
                string.IsNullOrWhiteSpace(user.Role))
                return null;

            var role = user.Role.Trim().ToLowerInvariant();
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
                    var artist = await _auth.FindArtistByCredentials(user.Email, user.Pwd);
                    if (artist == null) return null;
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
