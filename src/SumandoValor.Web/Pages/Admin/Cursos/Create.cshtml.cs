using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Admin.Cursos;

[Authorize(Roles = "Moderador,Admin")]
public class CreateModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(AppDbContext context, ILogger<CreateModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(150, ErrorMessage = "El título no puede exceder 150 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es requerida")]
        public string Descripcion { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "El público objetivo no puede exceder 500 caracteres")]
        public string? PublicoObjetivo { get; set; }

        public bool EsPublico { get; set; }

        public int? Orden { get; set; }
    }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var curso = new Curso
        {
            Titulo = Input.Titulo,
            Descripcion = Input.Descripcion,
            PublicoObjetivo = Input.PublicoObjetivo,
            EsPublico = Input.EsPublico,
            Orden = Input.Orden,
            Estado = EstatusCurso.Activo,
            FechaCreacion = DateTime.UtcNow
        };

        // Generar clave de acceso si el curso no es público
        if (!Input.EsPublico)
        {
            curso.ClaveAcceso = GenerateAccessKey();
        }

        _context.Cursos.Add(curso);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Curso {CursoId} creado por admin", curso.Id);
        
        if (!Input.EsPublico && !string.IsNullOrEmpty(curso.ClaveAcceso))
        {
            TempData["FlashSuccess"] = $"Curso creado exitosamente. Clave de acceso: {curso.ClaveAcceso}";
        }
        else
        {
            TempData["FlashSuccess"] = "Curso creado exitosamente.";
        }
        
        return RedirectToPage("/Admin/Cursos");
    }

    private static string GenerateAccessKey()
    {
        // Genera una clave de 8 caracteres alfanuméricos
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
