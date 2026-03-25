using AutoMapper;
using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Profiles
{
    public class AppointmentProfile : Profile
    {
        public AppointmentProfile()
        {
            CreateMap<AppointmentsWithoutIdDTO, Appointment>()
                .ForMember(d => d.Services, o => o.Ignore());

            CreateMap<Appointment, AppointmentsWithoutIdDTO>()
                .ForMember(d => d.ServiceIds, o => o.MapFrom(a => a.Services.Select(s => s.Id).ToList()));

            CreateMap<AppointmentsWithIdDTO, Appointment>()
                .ForMember(d => d.Services, o => o.Ignore());

            CreateMap<Appointment, AppointmentsWithIdDTO>()
                .ForMember(d => d.ServiceIds, o => o.MapFrom(a => a.Services.Select(s => s.Id).ToList()));
        }
    }
}
