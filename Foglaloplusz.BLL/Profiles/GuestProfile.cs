using AutoMapper;
using Foglaloplusz.BLL.DTOs.GuestDTOs;
using Foglaloplusz.DAL.Models;

namespace Foglaloplusz.BLL.Profiles
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
