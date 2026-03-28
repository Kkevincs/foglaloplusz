using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.DTOs.ArtistDTOs
{
    public class ArtistWithoutIdDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Pwd { get; set; }
        public int SalonId { get; set; }


        public string Description { get; set; }
    }
}
