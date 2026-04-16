using AutoMapper;
using Foglaloplusz.BLL.DTOs.SalonDTOs;
using Foglaloplusz.DAL.Models;

namespace Foglaloplusz.BLL.Profiles
{
    public class SalonProfile : Profile
    {
        public SalonProfile()
        {
            CreateMap<SalonWithoutIdDTO, Salon>()
                .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => src.Logo))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
                .ForMember(dest => dest.Owner, opt => opt.Ignore())
                .ForMember(dest => dest.Artists, opt => opt.Ignore())
                .ForMember(dest => dest.Appointments, opt => opt.Ignore());

            CreateMap<Salon, SalonWithoutIdDTO>()
                .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => src.LogoUrl));

            CreateMap<SalonWithIdDTO, Salon>()
                .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => src.Logo))
                .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
                .ForMember(dest => dest.Owner, opt => opt.Ignore())
                .ForMember(dest => dest.Artists, opt => opt.Ignore())
                .ForMember(dest => dest.Appointments, opt => opt.Ignore());

            CreateMap<Salon, SalonWithIdDTO>()
                .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => src.LogoUrl));

            CreateMap<Salon, SalonDirectoryCardDTO>()
                .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => src.LogoUrl));
        }
    }
}
