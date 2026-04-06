using AutoMapper;
using Salonic.BLL.DTOs.ServiceDTOs;
using Salonic.DAL.Models;

namespace Salonic.BLL.Profiles
{
    public class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            CreateMap<ServiceWithoutIdDTO, Service>()
                .ForMember(d => d.SalonId, o => o.Ignore())
                .ForMember(d => d.CreatedByArtistId, o => o.Ignore())
                .ForMember(d => d.Salon, o => o.Ignore())
                .ForMember(d => d.CreatedByArtist, o => o.Ignore())
                .ForMember(d => d.Artists, o => o.Ignore())
                .ForMember(d => d.Appointments, o => o.Ignore());

            CreateMap<Service, ServiceWithoutIdDTO>();

            CreateMap<ServiceWithIdDTO, Service>()
                .ForMember(d => d.SalonId, o => o.Ignore())
                .ForMember(d => d.CreatedByArtistId, o => o.Ignore())
                .ForMember(d => d.Salon, o => o.Ignore())
                .ForMember(d => d.CreatedByArtist, o => o.Ignore())
                .ForMember(d => d.Artists, o => o.Ignore())
                .ForMember(d => d.Appointments, o => o.Ignore());

            CreateMap<Service, ServiceWithIdDTO>();
        }
    }
}
