using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class ImagenesModel : PageModel
{
    private const long MaxBytes = 4 * 1024 * 1024; // 4MB
    private static readonly HashSet<string> AllowedExt = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ImagenesModel> _logger;

    public ImagenesModel(AppDbContext context, IWebHostEnvironment env, ILogger<ImagenesModel> logger)
    {
        _context = context;
        _env = env;
        _logger = logger;
    }

    public string? AboutMainUrl { get; set; }
    public string AboutMainAlt { get; set; } = "Fundación KPMG Venezuela";
    public string? WorkshopCardUrl { get; set; }
    public string WorkshopCardAlt { get; set; } = "Taller";

    public async Task OnGetAsync()
    {
        var img = await _context.SiteImages.AsNoTracking().FirstOrDefaultAsync(x => x.Key == "AboutMain");
        if (img != null)
        {
            AboutMainUrl = "/uploads/site/" + img.FileName;
            AboutMainAlt = img.AltText;
        }

        var w = await _context.SiteImages.AsNoTracking().FirstOrDefaultAsync(x => x.Key == "WorkshopCard");
        if (w != null)
        {
            WorkshopCardUrl = "/uploads/site/" + w.FileName;
            WorkshopCardAlt = w.AltText;
        }
    }

    public async Task<IActionResult> OnPostUploadAboutMainAsync(IFormFile file, string altText)
    {
        if (file == null || file.Length == 0)
        {
            TempData["FlashError"] = "Selecciona una imagen.";
            return RedirectToPage();
        }

        if (file.Length > MaxBytes)
        {
            TempData["FlashError"] = "La imagen supera el tamaño máximo permitido (4MB).";
            return RedirectToPage();
        }

        altText = (altText ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(altText))
        {
            TempData["FlashError"] = "El texto alternativo (alt) es obligatorio.";
            return RedirectToPage();
        }

        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExt.Contains(ext))
        {
            TempData["FlashError"] = "Formato no permitido. Usa jpg, png o webp.";
            return RedirectToPage();
        }

        await using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var bytes = ms.ToArray();
        if (!LooksLikeAllowedImage(bytes, ext))
        {
            TempData["FlashError"] = "La imagen no parece válida para el formato indicado.";
            return RedirectToPage();
        }

        var dir = Path.Combine(_env.WebRootPath, "uploads", "site");
        Directory.CreateDirectory(dir);

        var safeExt = ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ? ".jpg" : ext.ToLowerInvariant();
        var safeName = $"{Guid.NewGuid():N}{safeExt}";
        var physical = Path.Combine(dir, safeName);
        await System.IO.File.WriteAllBytesAsync(physical, bytes);

        // Replace previous
        var existing = await _context.SiteImages.FirstOrDefaultAsync(x => x.Key == "AboutMain");
        if (existing != null)
        {
            TryDeletePhysical(existing.FileName);
            existing.FileName = safeName;
            existing.AltText = altText;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.SiteImages.Add(new SiteImage
            {
                Key = "AboutMain",
                FileName = safeName,
                AltText = altText,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        TempData["FlashSuccess"] = "Imagen actualizada.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAboutMainAsync()
    {
        var existing = await _context.SiteImages.FirstOrDefaultAsync(x => x.Key == "AboutMain");
        if (existing == null)
        {
            TempData["FlashInfo"] = "No hay imagen para eliminar.";
            return RedirectToPage();
        }

        TryDeletePhysical(existing.FileName);
        _context.SiteImages.Remove(existing);
        await _context.SaveChangesAsync();
        TempData["FlashSuccess"] = "Imagen eliminada.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUploadWorkshopCardAsync(IFormFile file, string altText)
        => await UpsertSingleAsync("WorkshopCard", file, altText);

    public async Task<IActionResult> OnPostDeleteWorkshopCardAsync()
        => await DeleteSingleAsync("WorkshopCard");

    private async Task<IActionResult> UpsertSingleAsync(string key, IFormFile file, string altText)
    {
        if (file == null || file.Length == 0)
        {
            TempData["FlashError"] = "Selecciona una imagen.";
            return RedirectToPage();
        }

        if (file.Length > MaxBytes)
        {
            TempData["FlashError"] = "La imagen supera el tamaño máximo permitido (4MB).";
            return RedirectToPage();
        }

        altText = (altText ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(altText))
        {
            TempData["FlashError"] = "El texto alternativo (alt) es obligatorio.";
            return RedirectToPage();
        }

        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExt.Contains(ext))
        {
            TempData["FlashError"] = "Formato no permitido. Usa jpg, png o webp.";
            return RedirectToPage();
        }

        await using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var bytes = ms.ToArray();
        if (!LooksLikeAllowedImage(bytes, ext))
        {
            TempData["FlashError"] = "La imagen no parece válida para el formato indicado.";
            return RedirectToPage();
        }

        var dir = Path.Combine(_env.WebRootPath, "uploads", "site");
        Directory.CreateDirectory(dir);

        var safeExt = ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ? ".jpg" : ext.ToLowerInvariant();
        var safeName = $"{Guid.NewGuid():N}{safeExt}";
        var physical = Path.Combine(dir, safeName);
        await System.IO.File.WriteAllBytesAsync(physical, bytes);

        var existing = await _context.SiteImages.FirstOrDefaultAsync(x => x.Key == key);
        if (existing != null)
        {
            TryDeletePhysical(existing.FileName);
            existing.FileName = safeName;
            existing.AltText = altText;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.SiteImages.Add(new SiteImage
            {
                Key = key,
                FileName = safeName,
                AltText = altText,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        TempData["FlashSuccess"] = "Imagen actualizada.";
        return RedirectToPage();
    }

    private async Task<IActionResult> DeleteSingleAsync(string key)
    {
        var existing = await _context.SiteImages.FirstOrDefaultAsync(x => x.Key == key);
        if (existing == null)
        {
            TempData["FlashInfo"] = "No hay imagen para eliminar.";
            return RedirectToPage();
        }

        TryDeletePhysical(existing.FileName);
        _context.SiteImages.Remove(existing);
        await _context.SaveChangesAsync();
        TempData["FlashSuccess"] = "Imagen eliminada.";
        return RedirectToPage();
    }

    private void TryDeletePhysical(string fileName)
    {
        try
        {
            var physical = Path.Combine(_env.WebRootPath, "uploads", "site", fileName);
            if (System.IO.File.Exists(physical))
                System.IO.File.Delete(physical);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo eliminar archivo físico (SiteImage). File={File}", fileName);
        }
    }

    private static bool LooksLikeAllowedImage(byte[] bytes, string ext)
    {
        if (bytes.Length < 12) return false;

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (ext.Equals(".png", StringComparison.OrdinalIgnoreCase))
            return bytes.Length >= 8 &&
                   bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                   bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;

        // JPEG: FF D8 ... (we only check start)
        if (ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
            return bytes[0] == 0xFF && bytes[1] == 0xD8;

        // WebP: "RIFF" .... "WEBP"
        if (ext.Equals(".webp", StringComparison.OrdinalIgnoreCase))
            return bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
                   bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50;

        return false;
    }
}

