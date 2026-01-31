using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Http;

namespace SumandoValor.Web.Pages.Cursos;

public class AccessModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly ILogger<AccessModel> _logger;

    public AccessModel(AppDbContext context, ILogger<AccessModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Curso? Curso { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "La clave de acceso es requerida")]
        [StringLength(20, ErrorMessage = "La clave no puede exceder 20 caracteres")]
        public string ClaveAcceso { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(int id, string? token = null)
    {
        Curso = await _context.Cursos
            .FirstOrDefaultAsync(c => c.Id == id && c.Estado == EstatusCurso.Activo);

        if (Curso == null)
        {
            return NotFound();
        }

        // Si el curso es público, redirigir a la página normal
        if (Curso.EsPublico)
        {
            return RedirectToPage("/Cursos/Details", new { id });
        }

        // Si viene con token válido, guardar acceso y redirigir
        if (!string.IsNullOrEmpty(token) &&
            !string.IsNullOrEmpty(Curso.TokenAccesoUnico) &&
            Curso.TokenAccesoUnico.Equals(token, StringComparison.Ordinal) &&
            (Curso.TokenExpiracion == null || Curso.TokenExpiracion > DateTime.UtcNow))
        {
            HttpContext.Session.SetString($"curso_access_{id}", "granted");
            HttpContext.Response.Cookies.Append($"curso_token_{id}", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = !Request.IsHttps ? false : true,
                SameSite = SameSiteMode.Lax,
                Expires = Curso.TokenExpiracion ?? DateTimeOffset.UtcNow.AddYears(1) // Persistir por 1 año si no expira
            });
            return RedirectToPage("/Cursos/Details", new { id });
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        Curso = await _context.Cursos
            .FirstOrDefaultAsync(c => c.Id == id && c.Estado == EstatusCurso.Activo);

        if (Curso == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Verificar clave de acceso (case-insensitive para mejor UX)
        var claveIngresada = Input.ClaveAcceso?.Trim() ?? "";
        
        // Null safe check: si la clave del curso es nula, nadie entra (o todos entra, dependiendo logica, aquí asumimos seguridad: nadie).
        // Si el curso no tiene clave definida, logicamente no deberia pedirla, pero por seguridad fallamos.
        if (string.IsNullOrEmpty(Curso.ClaveAcceso) || 
            !Curso.ClaveAcceso.Equals(claveIngresada, StringComparison.OrdinalIgnoreCase))
        {
            // Log de intento fallido (sin exponer la clave)
            _logger.LogWarning("Intento de acceso fallido al curso {CursoId} con clave incorrecta", id);
            ModelState.AddModelError(nameof(Input.ClaveAcceso), "Clave de acceso incorrecta.");
            return Page();
        }

        // Guardar en sesión que el usuario tiene acceso
        HttpContext.Session.SetString($"curso_access_{id}", "granted");
        
        // También guardar en cookie para persistir después de login/registro
        if (!string.IsNullOrEmpty(Curso.TokenAccesoUnico))
        {
            HttpContext.Response.Cookies.Append($"curso_token_{id}", Curso.TokenAccesoUnico, new CookieOptions
            {
                HttpOnly = true,
                Secure = !Request.IsHttps ? false : true,
                SameSite = SameSiteMode.Lax,
                Expires = Curso.TokenExpiracion ?? DateTimeOffset.UtcNow.AddYears(1) // Persistir por 1 año si no expira
            });
        }

        _logger.LogInformation("Acceso concedido al curso {CursoId} con clave", id);
        
        // Si el usuario no está autenticado, redirigir a login con returnUrl para que después pueda acceder
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            var returnUrl = Url.Page("/Cursos/Details", new { id });
            return RedirectToPage("/Account/Login", new { returnUrl });
        }
        
        return RedirectToPage("/Cursos/Details", new { id });
    }
}
