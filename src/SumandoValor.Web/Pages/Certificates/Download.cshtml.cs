using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Certificates;

[Authorize]
public class DownloadModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DownloadModel> _logger;

    public DownloadModel(AppDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env, ILogger<DownloadModel> logger)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var cert = await _context.Certificados
            .Include(c => c.Taller)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cert == null)
            return NotFound();

        if (cert.Estado != EstadoCertificado.Aprobado)
            return Forbid();

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && cert.UserId != user.Id)
            return Forbid();

        if (string.IsNullOrWhiteSpace(cert.UrlPdf))
            return NotFound();

        // Only allow files under App_Data/Certificates
        var expectedPrefix = Path.Combine("App_Data", "Certificates") + Path.DirectorySeparatorChar;
        var normalized = cert.UrlPdf.Replace('/', Path.DirectorySeparatorChar);
        if (!normalized.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var physicalPath = Path.Combine(_env.ContentRootPath, normalized);
        if (!System.IO.File.Exists(physicalPath))
            return NotFound();

        var fileNameSafe = $"certificado_{cert.TallerId}_{DateTime.Now:yyyyMMdd}.pdf";
        _logger.LogInformation("Descarga de certificado CertId={CertId}, UserId={UserId}, Admin={IsAdmin}", cert.Id, user.Id, isAdmin);

        var bytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
        return File(bytes, "application/pdf", fileNameSafe);
    }
}

