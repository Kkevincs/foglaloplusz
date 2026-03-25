using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.DAL.Models.Interfaces
{
    public interface IUserModel
    {
        public int Id { get; set; }

        public string Email { get; set; }
        public string Role { get; set; }
        public string Pwd { get; set; }
    }
}
