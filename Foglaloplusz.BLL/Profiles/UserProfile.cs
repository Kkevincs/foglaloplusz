using AutoMapper;
using Foglaloplusz.BLL.DTOs.UserDTOs;
using Foglaloplusz.DAL.Models;

namespace Foglaloplusz.BLL.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserDTO, Guest>();
            CreateMap<UserDTO, Artist>();
            CreateMap<UserDTO, Owner>();
        }
    }
}
