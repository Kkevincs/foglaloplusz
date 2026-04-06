using System.ComponentModel.DataAnnotations;

namespace Salonic.presentation.Models
{
    public class SalonCreateForm
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        public string Theme { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        /// <summary>Optional logo image (JPEG, PNG, GIF, WebP).</summary>
        public IFormFile? Logo { get; set; }
    }
}
