using AutoMapper;
using Salonic.BLL.DTOs.UserDTOs;
using Salonic.BLL.Services.Interfaces;
using Salonic.DAL.Models.Interfaces;
using Salonic.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IMapper _mapper;
        private readonly IAuthenticationRepository _AuthRepository;

        public AuthenticationService(IMapper mapper, IAuthenticationRepository authRepository)
        {
            _mapper = mapper;
            _AuthRepository = authRepository;
        }

        public async Task<UserDTO> Authentication(UserDTO user)
        {
            var authUser = await _AuthRepository.Authentication(user.Email, user.Pwd, user.Role);
            if (authUser == null) return null;

            return new UserDTO
            {
                Id = authUser.Id,       
                Email = authUser.Email,   
                Role = user.Role
            };


        }
    }
}
