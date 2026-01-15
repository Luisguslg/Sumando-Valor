using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Profile;

[Authorize(Roles = "Beneficiario")]
public class TalleresModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TalleresModel(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public List<Inscripcion> Inscripciones { get; set; } = new();
    public Dictionary<int, EncuestaSatisfaccion> EncuestasByTallerId { get; set; } = new();
    public Dictionary<int, Certificado> CertificadosByTallerId { get; set; } = new();

    [BindProperty]
    public EncuestaInputModel EncuestaInput { get; set; } = new();

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return;
        }

        Inscripciones = await _context.Inscripciones
            .Include(i => i.Taller)
                .ThenInclude(t => t.Curso)
            .Where(i => i.UserId == user.Id && i.Estado == EstadoInscripcion.Activa)
            .OrderByDescending(i => i.Taller.FechaInicio)
            .ToListAsync();

        var tallerIds = Inscripciones.Select(i => i.TallerId).Distinct().ToList();
        if (tallerIds.Count == 0)
        {
            return;
        }

        EncuestasByTallerId = await _context.EncuestasSatisfaccion
            .Where(e => e.UserId == user.Id && tallerIds.Contains(e.TallerId))
            .ToDictionaryAsync(e => e.TallerId);

        CertificadosByTallerId = await _context.Certificados
            .Where(c => c.UserId == user.Id && tallerIds.Contains(c.TallerId))
            .ToDictionaryAsync(c => c.TallerId);
    }

    public async Task<IActionResult> OnPostSubmitEncuestaAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["FlashError"] = "Debes iniciar sesión para completar la encuesta.";
            return RedirectToPage();
        }

        if (EncuestaInput.Rating1_5 < 1 || EncuestaInput.Rating1_5 > 5)
        {
            TempData["FlashError"] = "Selecciona una calificación válida (1 a 5).";
            return RedirectToPage();
        }

        var taller = await _context.Talleres.FirstOrDefaultAsync(t => t.Id == EncuestaInput.TallerId);
        if (taller == null)
        {
            TempData["FlashError"] = "Taller no encontrado.";
            return RedirectToPage();
        }

        if (taller.Estatus != EstatusTaller.Finalizado)
        {
            TempData["FlashError"] = "La encuesta solo está disponible cuando el taller está finalizado.";
            return RedirectToPage();
        }

        if (!taller.RequiereEncuesta)
        {
            TempData["FlashInfo"] = "Este taller no requiere encuesta.";
            return RedirectToPage();
        }

        var inscrito = await _context.Inscripciones.AnyAsync(i =>
            i.TallerId == taller.Id && i.UserId == user.Id && i.Estado == EstadoInscripcion.Activa);

        if (!inscrito)
        {
            TempData["FlashError"] = "Solo puedes responder encuestas de talleres en los que estuviste inscrito.";
            return RedirectToPage();
        }

        var already = await _context.EncuestasSatisfaccion.AnyAsync(e => e.TallerId == taller.Id && e.UserId == user.Id);
        if (already)
        {
            TempData["FlashInfo"] = "Ya respondiste la encuesta de este taller.";
            return RedirectToPage();
        }

        var encuesta = new EncuestaSatisfaccion
        {
            TallerId = taller.Id,
            UserId = user.Id,
            Rating1_5 = EncuestaInput.Rating1_5,
            Comentario = string.IsNullOrWhiteSpace(EncuestaInput.Comentario) ? null : EncuestaInput.Comentario.Trim(),
            ScorePromedio = EncuestaInput.Rating1_5,
            PayloadJson = string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _context.EncuestasSatisfaccion.Add(encuesta);
        await _context.SaveChangesAsync();

        TempData["FlashSuccess"] = "¡Gracias! Tu encuesta fue enviada correctamente.";
        return RedirectToPage();
    }

    public sealed class EncuestaInputModel
    {
        public int TallerId { get; set; }
        public int Rating1_5 { get; set; }
        public string? Comentario { get; set; }
    }
}
