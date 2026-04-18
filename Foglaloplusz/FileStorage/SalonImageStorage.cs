using Foglaloplusz.BLL.Services.Interfaces;

namespace Foglaloplusz.FileStorage
{
    public interface ISalonImageStorage
    {
        /// <summary>Saves an uploaded logo and returns a site-relative URL (e.g. /uploads/salons/...), or null if no file.</summary>
        Task<string?> SaveSalonLogoAsync(IFormFile? file, CancellationToken cancellationToken = default);
    }

    public class SalonImageStorage : ISalonImageStorage, ISalonLogoCleanup
    {
        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private const long MaxBytes = 5 * 1024 * 1024;

        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<SalonImageStorage> _logger;

        public SalonImageStorage(IWebHostEnvironment environment, ILogger<SalonImageStorage> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string?> SaveSalonLogoAsync(IFormFile? file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                return null;

            if (file.Length > MaxBytes)
                throw new ArgumentException("Logo file is too large. Maximum size is 5 MB.");

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                throw new ArgumentException("Logo must be a JPEG, PNG, GIF, or WebP image.");

            var webRoot = string.IsNullOrEmpty(_environment.WebRootPath)
                ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                : _environment.WebRootPath;

            var uploadsDir = Path.Combine(webRoot, "uploads", "salons");
            Directory.CreateDirectory(uploadsDir);

            var storedName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
            var fullPath = Path.Combine(uploadsDir, storedName);

            await using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            _logger.LogInformation("Stored salon logo at {Path}", fullPath);

            return $"/uploads/salons/{storedName}";
        }

        public Task TryDeleteByRelativeUrlAsync(string? relativeUrl, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(relativeUrl))
                return Task.CompletedTask;

            var trimmed = relativeUrl.Trim();
            const string prefix = "/uploads/salons/";
            if (!trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            var fileName = trimmed[prefix.Length..];
            if (fileName.Length == 0 || fileName.Contains("..", StringComparison.Ordinal) || Path.IsPathRooted(fileName))
                return Task.CompletedTask;

            var webRoot = string.IsNullOrEmpty(_environment.WebRootPath)
                ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                : _environment.WebRootPath;

            var salonsDir = Path.GetFullPath(Path.Combine(webRoot, "uploads", "salons"));
            var fullPath = Path.GetFullPath(Path.Combine(salonsDir, fileName));

            if (!fullPath.StartsWith(salonsDir, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
                return Task.CompletedTask;

            try
            {
                File.Delete(fullPath);
                _logger.LogInformation("Deleted salon logo file {Path}", fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete salon logo at {Path}", fullPath);
            }

            return Task.CompletedTask;
        }
    }
}
