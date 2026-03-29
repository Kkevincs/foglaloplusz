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
        public Task<List<ArtistWithIdDTO>> GetAll();
        /// <summary>Artists in the salon if <paramref name="salonId"/> belongs to <paramref name="ownerId"/>; otherwise null.</summary>
        Task<List<ArtistWithIdDTO>?> GetBySalonIdForOwner(int ownerId, int salonId);
        public Task<ArtistWithIdDTO> GetById(int id);
        public Task<ArtistWithoutIdDTO> Create(ArtistWithoutIdDTO artist);
        public Task Update(ArtistWithIdDTO artist);
        public Task DeleteById(int id);
    }
}
