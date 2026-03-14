using Salonic.BLL.DTOs.UserDTOs;
using Salonic.DAL.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Services.Interfaces
{
    public interface IAuthenticationService
    {
        public Task<UserDTO> Authentication (UserDTO user);
            
        
    }
}
