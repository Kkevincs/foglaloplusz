using Foglaloplusz.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foglaloplusz.BLL.DTOs.UserDTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Pwd { get; set; }

        /// <summary>Szalon azonosító szakember bejelentkezéshez; meg kell egyeznie a szakember <c>SalonId</c> mezőjével.</summary>
        public int? SalonId { get; set; }
    }
}
