using AutoMapper;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Salonic.BLL.DTOs.UserDTOs;
using Salonic.DAL.Models;
using Salonic.DAL.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<IUserModel, UserDTO>();
            CreateMap<UserDTO, IUserModel>();
            CreateMap<UserDTO, Guest>();
            CreateMap<Guest, UserDTO>();
            CreateMap<UserDTO, Artist>();
            CreateMap<Artist, UserDTO>();

        }
    }
}
