using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Infrastructure.Data;
using System.IO;

namespace SumandoValor.Web.Services;

public class UploadCleanupService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<UploadCleanupService> _logger;

    public UploadCleanupService(IWebHostEnvironment env, ILogger<UploadCleanupService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task CleanOrphanUploadsAsync(AppDbContext context, bool isDevelopment)
    {
        try
        {
            // Limpieza automática solo en desarrollo por seguridad.
            // En producción, se puede habilitar manualmente o mediante configuración.
            if (!isDevelopment)
            {
                _logger.LogDebug("Limpieza de uploads huérfanos omitida (solo en desarrollo por defecto).");
                return;
            }

            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads");
            var carouselDir = Path.Combine(uploadsRoot, "carousel");
            var siteDir = Path.Combine(uploadsRoot, "site");

            var referencedCarousel = await context.CarouselItems.Select(x => x.FileName).ToListAsync();
            var referencedSite = await context.SiteImages.Select(x => x.FileName).ToListAsync();
            var referenced = new HashSet<string>(referencedCarousel.Concat(referencedSite), StringComparer.OrdinalIgnoreCase);

            void TryDeleteOrphan(string dir)
            {
                if (!Directory.Exists(dir)) return;

                foreach (var f in Directory.EnumerateFiles(dir))
                {
                    var fileName = Path.GetFileName(f);
                    if (string.IsNullOrWhiteSpace(fileName)) continue;
                    if (fileName.Equals(".gitkeep", StringComparison.OrdinalIgnoreCase)) continue;
                    if (referenced.Contains(fileName)) continue;

                    try
                    {
                        File.Delete(f);
                        _logger.LogInformation("Deleted orphan upload file: {File}", f);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed deleting orphan upload file: {File}", f);
                    }
                }
            }

            TryDeleteOrphan(carouselDir);
            TryDeleteOrphan(siteDir);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during CleanOrphanUploadsAsync");
        }
    }
}
