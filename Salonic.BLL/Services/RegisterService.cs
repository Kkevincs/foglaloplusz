using AutoMapper;
using Salonic.BLL.DTOs.UserDTOs;
using Salonic.BLL.Services.Interfaces;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories;
using Salonic.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly IGuestRepository _guestrepo;
        private readonly IArtistRepository _artistrepo;
        private readonly IMapper _mapper;

        public RegisterService(IGuestRepository guestrepo, IArtistRepository artistrepo, IMapper mapper)
        {
            _guestrepo = guestrepo;
            _artistrepo = artistrepo;
            _mapper = mapper;
        }

        public async Task<UserDTO> Register(UserDTO user)
        {

            switch (user.Role)
            {
                case "guest":
                    var regGuest = _mapper.Map<Guest>(user);
                    await _guestrepo.Create(regGuest);
                    return user;
                    break;
                case "artist":
                    var regArtist = _mapper.Map<Artist>(user);
                    await _artistrepo.Create(regArtist);
                    return user;
                    break;
                default:
                    return null;

            }
        }
    }
}
