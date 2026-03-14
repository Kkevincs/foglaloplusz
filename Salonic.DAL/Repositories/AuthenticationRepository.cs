using Microsoft.EntityFrameworkCore;
using Salonic.DAL.Data;
using Salonic.DAL.Models.Interfaces;
using Salonic.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Salonic.DAL.Repositories
{
    public class AuthenticationRepository : IAuthenticationRepository
    {

        private readonly AppDbContext _appDbContext;

        public AuthenticationRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IUserModel?> Authentication(string username, string password, string role)
        {
            switch (role)
            {
                case "guest":
                    return await _appDbContext.Guests
                        .FirstOrDefaultAsync(x => x.Name == username && x.Pwd == password);
                case "artist":
                    return await _appDbContext.Artists
                        .FirstOrDefaultAsync(x => x.Name == username && x.Pwd == password);
                case "owner":
                    return await _appDbContext.Owners
                        .FirstOrDefaultAsync(x => x.Name == username && x.Pwd == password);
                case "admin":
                    return await _appDbContext.Admins
                        .FirstOrDefaultAsync(x => x.Name == username && x.Pwd == password);
                default:
                    return null;
            }

        }
    }
}
