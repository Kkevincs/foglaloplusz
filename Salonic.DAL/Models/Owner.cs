using Salonic.DAL.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.DAL.Models
{
    public class Owner : IUserModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public ICollection<Salon> Salons { get; set; } = new List<Salon>();
        public string Role { get; set; } = "Owner";
        public string Pwd { get; set; }
    }
}
