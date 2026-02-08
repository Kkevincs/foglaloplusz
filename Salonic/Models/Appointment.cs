using System.ComponentModel.DataAnnotations.Schema;

namespace Salonic.Models
{
    public class Appointment
    {
        public int ID { get; set; }

        public int ServiceID {get; set;}
        public int wokerID {get; set;}
        public int year { get; set; }
        public int month { get; set; }
        public int day { get; set; }
        public int hour { get; set; }
        public int minute { get; set; }

        public string customerName { get; set; }

        public string emailAddress { get; set; }
        public string phoneNumber { get; set; }

        public string notes { get; set; }

        public int durationInMinutes { get; set; }
    }
}
