using System.Security.Cryptography.X509Certificates;

namespace Salonic.Models
{
    public class Service
    {
        public int ID { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int price { get; set; }
    }
}
