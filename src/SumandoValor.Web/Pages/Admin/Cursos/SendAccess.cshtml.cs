using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;
using SumandoValor.Infrastructure.Services;

namespace SumandoValor.Web.Pages.Admin.Cursos;

[Authorize(Roles = "Moderador,Admin")]
public class SendAccessModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendAccessModel> _logger;

    public SendAccessModel(
        AppDbContext context,
        IEmailService emailService,
        ILogger<SendAccessModel> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public Curso? Curso { get; set; }
    public List<ApplicationUser> Usuarios { get; set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int CursoId { get; set; }

        [Required(ErrorMessage = "Ingresa al menos un correo electrónico")]
        public string Emails { get; set; } = string.Empty;

        [Range(1, 365, ErrorMessage = "Los días de expiración deben estar entre 1 y 365")]
        public int DiasExpiracion { get; set; } = 30;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Curso = await _context.Cursos
            .FirstOrDefaultAsync(c => c.Id == id);

        if (Curso == null)
        {
            return NotFound();
        }

        if (Curso.EsPublico)
        {
            TempData["FlashError"] = "Este Programa Formativo es público y no requiere enlace de acceso.";
            return RedirectToPage("/Admin/Cursos");
        }

        // Cargar usuarios registrados
        Usuarios = await _context.Users
            .OrderBy(u => u.Nombres)
            .ThenBy(u => u.Apellidos)
            .ToListAsync();

        Input.CursoId = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Curso = await _context.Cursos
            .FirstOrDefaultAsync(c => c.Id == Input.CursoId);

        if (Curso == null)
        {
            return NotFound();
        }

        // Parsear emails (líneas, comas o punto y coma)
        var emailList = (Input.Emails ?? "")
            .Split(new[] { '\n', '\r', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim())
            .Where(e => e.Length > 0 && e.Contains('@'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (emailList.Count == 0)
        {
            ModelState.AddModelError(nameof(Input.Emails), "Ingresa al menos un correo electrónico válido.");
            Usuarios = await _context.Users
                .OrderBy(u => u.Nombres)
                .ThenBy(u => u.Apellidos)
                .ToListAsync();
            return Page();
        }

        if (!ModelState.IsValid)
        {
            Usuarios = await _context.Users
                .OrderBy(u => u.Nombres)
                .ThenBy(u => u.Apellidos)
                .ToListAsync();
            return Page();
        }

        // Generar token único (compartido para todos los destinatarios)
        var token = GenerateUniqueToken();
        Curso.TokenAccesoUnico = token;
        Curso.TokenExpiracion = DateTime.UtcNow.AddDays(Input.DiasExpiracion);

        await _context.SaveChangesAsync();

        var accessLink = Url.Page(
            "/Cursos/Details",
            pageHandler: null,
            values: new { id = Curso.Id, token = token },
            protocol: Request.Scheme) ?? string.Empty;

        var successCount = 0;
        var errors = new List<string>();

        try
        {
            await _emailService.SendCourseAccessLinkToMultipleAsync(emailList, Curso.Titulo, accessLink);
            successCount = emailList.Count;
            _logger.LogInformation("Enlace de acceso enviado para curso {CursoId} a {Count} destinatarios", Curso.Id, successCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando enlaces de acceso para curso {CursoId}", Curso.Id);
            TempData["FlashError"] = $"Error al enviar los correos: {ex.Message}";
            return RedirectToPage("/Admin/Cursos/SendAccess", new { id = Curso.Id });
        }

        TempData["FlashSuccess"] = successCount == 1
            ? $"Enlace enviado exitosamente a {emailList[0]}"
            : $"Enlace enviado exitosamente a {successCount} destinatarios.";

        return RedirectToPage("/Admin/Cursos");
    }

    private static string GenerateUniqueToken()
    {
        return Guid.NewGuid().ToString("N");
    }
}
