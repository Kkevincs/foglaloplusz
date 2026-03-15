using Salonic.BLL.DTOs.UserDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Services.Interfaces
{
    public interface IRegisterService
    {
        public Task<UserDTO> Register(UserDTO user);
    }
}
