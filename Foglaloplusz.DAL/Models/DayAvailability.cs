using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foglaloplusz.DAL.Models
{
    public class DayAvailability
    {
        public DateTime Date { get; set; }
        public bool HasAvailableAppointments { get; set; }

        
    }
}
