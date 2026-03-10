using AutoMapper;
using Salonic.BLL.DTOs.ArtistDTOs;
using Salonic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Profiles
{
    public class ArtistProfile : Profile
    {
        public ArtistProfile()
        {
            CreateMap<AddArtist, Artist>();
            CreateMap<Artist, AddArtist>();
            CreateMap<GetArtist, Artist>();
            CreateMap<Artist, GetArtist>();
        }
    }
}
