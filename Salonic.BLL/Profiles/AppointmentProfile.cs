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
            CreateMap<AppointmentsWithoutIdDTO, Appointment>(); 
            CreateMap<Appointment, AppointmentsWithoutIdDTO>(); 
            CreateMap<AppointmentsWithIdDTO, Appointment>();    
            CreateMap<Appointment, AppointmentsWithIdDTO>();
        }
    }
}
