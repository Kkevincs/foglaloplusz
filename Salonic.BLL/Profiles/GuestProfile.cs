using AutoMapper;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Salonic.BLL.DTOs.GuestDTOs;
using Salonic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Profiles
{
    public class GuestProfile : Profile
    {
        public GuestProfile()
        {
            CreateMap<GuestWithoutIdDTO, Guest>();
            CreateMap<Guest, GuestWithoutIdDTO>();
            CreateMap<GuestWithIdDTO, Guest>();
            CreateMap<Guest, GuestWithIdDTO>();
        }
    }
}
