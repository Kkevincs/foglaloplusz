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
            CreateMap<SalonWithoutIdDTO, Salon>()
                .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => src.Logo));
            CreateMap<Salon, SalonWithoutIdDTO>()
                .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => src.LogoUrl))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => string.Empty));

            CreateMap<SalonWithIdDTO, Salon>()
                .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => src.Logo));
            CreateMap<Salon, SalonWithIdDTO>()
                .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => src.LogoUrl))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => string.Empty));
        }
    }
}
