using AutoMapper;
using Salonic.BLL.DTOs.GuestDTOs;
using Salonic.DAL.Models;

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
