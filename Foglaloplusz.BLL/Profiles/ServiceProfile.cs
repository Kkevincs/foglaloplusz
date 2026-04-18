using AutoMapper;
using Foglaloplusz.BLL.DTOs.ServiceDTOs;
using Foglaloplusz.DAL.Models;

namespace Foglaloplusz.BLL.Profiles
{
    public class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            CreateMap<ServiceWithoutIdDTO, Service>()
                .ForMember(d => d.SalonId, o => o.Ignore())
                .ForMember(d => d.OwnerId, o => o.Ignore())
                .ForMember(d => d.ArtistId, o => o.Ignore())
                .ForMember(d => d.Salon, o => o.Ignore())
                .ForMember(d => d.Owner, o => o.Ignore())
                .ForMember(d => d.Artist, o => o.Ignore())
                .ForMember(d => d.Offerings, o => o.Ignore())
                .ForMember(d => d.Appointments, o => o.Ignore());

            CreateMap<Service, ServiceWithoutIdDTO>();

            CreateMap<ServiceWithIdDTO, Service>()
                .ForMember(d => d.SalonId, o => o.Ignore())
                .ForMember(d => d.OwnerId, o => o.Ignore())
                .ForMember(d => d.ArtistId, o => o.Ignore())
                .ForMember(d => d.Salon, o => o.Ignore())
                .ForMember(d => d.Owner, o => o.Ignore())
                .ForMember(d => d.Artist, o => o.Ignore())
                .ForMember(d => d.Offerings, o => o.Ignore())
                .ForMember(d => d.Appointments, o => o.Ignore());

            CreateMap<Service, ServiceWithIdDTO>()
                .ForMember(d => d.OwnerArtistId, o => o.MapFrom(s => s.ArtistId))
                .ForMember(d => d.OfferingId, o => o.Ignore())
                .ForMember(d => d.IsImported, o => o.Ignore());
        }
    }
}
