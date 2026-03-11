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
            CreateMap<AppointmentsWithoutIdDTO, AppointmentProfile>();
            CreateMap<AppointmentProfile, AppointmentsWithoutIdDTO>();
            CreateMap<AppointmentsWithIdDTO, AppointmentProfile>();
            CreateMap<AppointmentProfile, AppointmentsWithIdDTO>();
        }
    }
}
