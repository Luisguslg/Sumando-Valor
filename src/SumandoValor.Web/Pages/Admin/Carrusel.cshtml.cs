using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class CarruselModel : PageModel
{
    private const long MaxBytes = 4 * 1024 * 1024; // 4MB
    private static readonly HashSet<string> AllowedExt = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<CarruselModel> _logger;

    public CarruselModel(AppDbContext context, IWebHostEnvironment env, ILogger<CarruselModel> logger)
    {
        _context = context;
        _env = env;
        _logger = logger;
    }

    public List<Row> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        var items = await _context.CarouselItems
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.Id)
            .ToListAsync();

        Items = items.Select((x, idx) => new Row
        {
            Id = x.Id,
            FileName = x.FileName,
            AltText = x.AltText,
            SortOrder = x.SortOrder,
            IsActive = x.IsActive,
            CanMoveUp = idx > 0,
            CanMoveDown = idx < items.Count - 1
        }).ToList();
    }

    public async Task<IActionResult> OnPostUploadAsync(IFormFile file, string altText)
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

        // Basic magic-byte validation (anti-path-traversal/anti-fake extension)
        await using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var bytes = ms.ToArray();
        if (!LooksLikeAllowedImage(bytes, ext))
        {
            TempData["FlashError"] = "La imagen no parece válida para el formato indicado.";
            return RedirectToPage();
        }

        var dir = Path.Combine(_env.WebRootPath, "uploads", "carousel");
        Directory.CreateDirectory(dir);

        var safeName = $"{Guid.NewGuid():N}{(ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ? ".jpg" : ext.ToLowerInvariant())}";
        var physical = Path.Combine(dir, safeName);
        await System.IO.File.WriteAllBytesAsync(physical, bytes);

        var nextOrder = (await _context.CarouselItems.MaxAsync(x => (int?)x.SortOrder) ?? 0) + 1;
        _context.CarouselItems.Add(new CarouselItem
        {
            FileName = safeName,
            AltText = altText,
            SortOrder = nextOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        TempData["FlashSuccess"] = "Imagen cargada al carrusel.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int id)
    {
        var item = await _context.CarouselItems.FirstOrDefaultAsync(x => x.Id == id);
        if (item == null)
        {
            TempData["FlashError"] = "Elemento no encontrado.";
            return RedirectToPage();
        }

        item.IsActive = !item.IsActive;
        await _context.SaveChangesAsync();
        TempData["FlashSuccess"] = item.IsActive ? "Elemento activado." : "Elemento desactivado.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveUpAsync(int id)
    {
        var items = await _context.CarouselItems.OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync();
        var idx = items.FindIndex(x => x.Id == id);
        if (idx <= 0)
            return RedirectToPage();

        (items[idx - 1].SortOrder, items[idx].SortOrder) = (items[idx].SortOrder, items[idx - 1].SortOrder);
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveDownAsync(int id)
    {
        var items = await _context.CarouselItems.OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync();
        var idx = items.FindIndex(x => x.Id == id);
        if (idx < 0 || idx >= items.Count - 1)
            return RedirectToPage();

        (items[idx + 1].SortOrder, items[idx].SortOrder) = (items[idx].SortOrder, items[idx + 1].SortOrder);
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var item = await _context.CarouselItems.FirstOrDefaultAsync(x => x.Id == id);
        if (item == null)
        {
            TempData["FlashError"] = "Elemento no encontrado.";
            return RedirectToPage();
        }

        try
        {
            var physical = Path.Combine(_env.WebRootPath, "uploads", "carousel", item.FileName);
            if (System.IO.File.Exists(physical))
                System.IO.File.Delete(physical);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo eliminar archivo físico del carrusel. Id={Id}", id);
        }

        _context.CarouselItems.Remove(item);
        await _context.SaveChangesAsync();
        TempData["FlashSuccess"] = "Elemento eliminado.";
        return RedirectToPage();
    }

    private static bool LooksLikeAllowedImage(byte[] bytes, string ext)
    {
        if (bytes.Length < 12) return false;

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (ext.Equals(".png", StringComparison.OrdinalIgnoreCase))
            return bytes.Length >= 8 &&
                   bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                   bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;

        // JPEG: FF D8 ... FF D9 (we only check start)
        if (ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
            return bytes[0] == 0xFF && bytes[1] == 0xD8;

        // WebP: "RIFF" .... "WEBP"
        if (ext.Equals(".webp", StringComparison.OrdinalIgnoreCase))
            return bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
                   bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50;

        return false;
    }

    public sealed class Row
    {
        public int Id { get; set; }
        public string FileName { get; set; } = "";
        public string AltText { get; set; } = "";
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public bool CanMoveUp { get; set; }
        public bool CanMoveDown { get; set; }
    }
}

