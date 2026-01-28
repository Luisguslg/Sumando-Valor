using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
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

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; } = string.Empty;

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

        if (!ModelState.IsValid)
        {
            Usuarios = await _context.Users
                .OrderBy(u => u.Nombres)
                .ThenBy(u => u.Apellidos)
                .ToListAsync();
            return Page();
        }

        // Generar token único (cada vez que se envía, se genera uno nuevo, invalidando el anterior)
        var token = GenerateUniqueToken();
        Curso.TokenAccesoUnico = token;
        Curso.TokenExpiracion = DateTime.UtcNow.AddDays(Input.DiasExpiracion);

        await _context.SaveChangesAsync();

        // Generar enlace de acceso
        var accessLink = Url.Page(
            "/Cursos/Details",
            pageHandler: null,
            values: new { id = Curso.Id, token = token },
            protocol: Request.Scheme);

        try
        {
            await _emailService.SendCourseAccessLinkAsync(
                Input.Email,
                Curso.Titulo,
                accessLink ?? string.Empty);

            _logger.LogInformation("Enlace de acceso enviado para curso {CursoId} a {Email}", Curso.Id, Input.Email);
            TempData["FlashSuccess"] = $"Enlace de acceso enviado exitosamente a {Input.Email}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando enlace de acceso para curso {CursoId} a {Email}", Curso.Id, Input.Email);
            TempData["FlashError"] = $"Error al enviar el email: {ex.Message}";
        }

        return RedirectToPage("/Admin/Cursos");
    }

    private static string GenerateUniqueToken()
    {
        // Genera un token seguro de 32 caracteres
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[24];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "")
            .Substring(0, 32);
    }
}
