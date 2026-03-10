using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.BLL.DTOs.ArtistDTOs;
using Salonic.BLL.DTOs.ServiceDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Services.Interfaces
{
    public interface IArtistService
    {
        public Task<List<GetArtist>> GetAll();
        public Task<GetArtist> GetById(int id);
        public Task<AddArtist> Create(AddArtist artist);
        public Task Update(GetArtist artist);
        public Task DeleteById(int id);
    }
}
