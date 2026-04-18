using System.ComponentModel.DataAnnotations;

namespace Foglaloplusz.presentation.Models
{
    public class SalonCreateForm
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        public string Theme { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? Location { get; set; }

        /// <summary>Optional logo image (JPEG, PNG, GIF, WebP).</summary>
        public IFormFile? Logo { get; set; }
    }
}
