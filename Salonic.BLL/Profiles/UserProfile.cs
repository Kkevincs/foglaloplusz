using AutoMapper;
using Salonic.BLL.DTOs.UserDTOs;
using Salonic.DAL.Models;

namespace Salonic.BLL.Profiles
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
