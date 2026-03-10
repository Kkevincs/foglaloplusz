using AutoMapper;
using Salonic.BLL.DTOs.ServiceDTOs;
using Salonic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Profiles
{
    public class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            CreateMap<AddService, Service>();
            CreateMap<Service, AddService>();
            CreateMap<GetService, Service>();
            CreateMap<Service, GetService>();
        }
    }
}
