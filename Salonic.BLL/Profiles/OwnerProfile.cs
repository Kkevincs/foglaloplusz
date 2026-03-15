using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Salonic.BLL.DTOs.ArtistDTOs;
using Salonic.BLL.DTOs.OwnerDTOs;
using Salonic.DAL.Models;

namespace Salonic.BLL.Profiles
{
    public class OwnerProfile : Profile
    {
        public OwnerProfile()
        {
            CreateMap<OwnerWithoutIdDTO, Owner>();
            CreateMap<Owner, OwnerWithoutIdDTO>();
            CreateMap<OwnerWithIdDTO, Owner>();
            CreateMap<Owner, OwnerWithIdDTO>();
        }
    }
}
