namespace Salonic.BLL.Services.Interfaces
{
    /// <summary>Removes uploaded salon logo files from disk after DB delete.</summary>
    public interface ISalonLogoCleanup
    {
        Task TryDeleteByRelativeUrlAsync(string? relativeUrl, CancellationToken cancellationToken = default);
    }
}
