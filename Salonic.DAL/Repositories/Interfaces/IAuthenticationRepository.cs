using Salonic.DAL.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.DAL.Repositories.Interfaces
{
    public interface IAuthenticationRepository
    {
        public Task<IUserModel?> Authentication(string email, string password, string role);
    }
}
