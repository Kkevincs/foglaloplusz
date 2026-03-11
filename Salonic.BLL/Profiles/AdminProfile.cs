using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Salonic.BLL.DTOs.AdminDTOs;
using Salonic.BLL.DTOs.ArtistDTOs;
using Salonic.DAL.Models;

namespace Salonic.BLL.Profiles
{
    public class AdminProfile : Profile
    {
        public AdminProfile()
        {
            CreateMap<AdminWithoutIdDTO, Admin>();
            CreateMap<Admin, AdminWithoutIdDTO>();
            CreateMap<AdminWithIdDTO, Admin>();
            CreateMap<Admin, AdminWithIdDTO>();
        }
    }
}
