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
            CreateMap<AddAppointment, AppointmentProfile>();
            CreateMap<AppointmentProfile, AddAppointment>();
            CreateMap<GetAppointment, AppointmentProfile>();
            CreateMap<AppointmentProfile, GetAppointment>();
        }
    }
}
