using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;

namespace Salonic.Models
{
    public class Service
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public int? Price { get; set; }        
        public int WorkerId { get; set; }
        [ForeignKey(nameof(WorkerId))]
        [InverseProperty(nameof(Worker.Services))]
        public List<Worker> Workers {  get; set; }
    }
}
