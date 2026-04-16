using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Foglaloplusz.BLL.DTOs.ArtistDTOs;
using Foglaloplusz.BLL.DTOs.OwnerDTOs;
using Foglaloplusz.DAL.Models;

namespace Foglaloplusz.BLL.Profiles
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
