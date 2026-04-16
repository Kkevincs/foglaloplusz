using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foglaloplusz.BLL.DTOs.OwnerDTOs
{
    public class OwnerWithIdDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Pwd { get; set; } = string.Empty;
        public string Email { get; set; }
    }
}
