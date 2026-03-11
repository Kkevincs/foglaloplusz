using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Salonic.BLL.DTOs.ArtistDTOs;
using Salonic.BLL.DTOs.SalonDTOs;
using Salonic.DAL.Models;

namespace Salonic.BLL.Profiles
{
    public class SalonProfile : Profile
    {
        public SalonProfile()
        {
            CreateMap<SalonWithoutIdDTO, Salon>();
            CreateMap<Salon, SalonWithoutIdDTO>();
            CreateMap<SalonWithIdDTO, Salon>();
            CreateMap<Salon, SalonWithIdDTO>();
        }
    }
}
