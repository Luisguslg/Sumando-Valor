using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;
using Microsoft.AspNetCore.Session;

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

    public async Task<IActionResult> OnGetAsync(int id)
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

        // Verificar clave de acceso (comparación case-sensitive para mayor seguridad)
        if (string.IsNullOrEmpty(Curso.ClaveAcceso) || 
            !Curso.ClaveAcceso.Equals(Input.ClaveAcceso.Trim(), StringComparison.Ordinal))
        {
            // Log de intento fallido (sin exponer la clave)
            _logger.LogWarning("Intento de acceso fallido al curso {CursoId} con clave incorrecta", id);
            ModelState.AddModelError(nameof(Input.ClaveAcceso), "Clave de acceso incorrecta.");
            return Page();
        }

        // Guardar en sesión que el usuario tiene acceso (con timeout de 2 horas)
        HttpContext.Session.SetString($"curso_access_{id}", "granted");

        _logger.LogInformation("Acceso concedido al curso {CursoId} con clave", id);
        return RedirectToPage("/Cursos/Details", new { id });
    }
}
