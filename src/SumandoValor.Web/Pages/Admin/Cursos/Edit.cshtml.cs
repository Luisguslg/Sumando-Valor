using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Admin.Cursos;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly ILogger<EditModel> _logger;

    public EditModel(AppDbContext context, ILogger<EditModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Curso? Curso { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(150, ErrorMessage = "El título no puede exceder 150 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es requerida")]
        public string Descripcion { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "El público objetivo no puede exceder 500 caracteres")]
        public string? PublicoObjetivo { get; set; }

        public bool EsPublico { get; set; }

        public int? Orden { get; set; }

        [Required]
        public EstatusCurso Estado { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id);

        if (Curso == null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = Curso.Id,
            Titulo = Curso.Titulo,
            Descripcion = Curso.Descripcion,
            PublicoObjetivo = Curso.PublicoObjetivo,
            EsPublico = Curso.EsPublico,
            Orden = Curso.Orden,
            Estado = Curso.Estado
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == Input.Id);
            return Page();
        }

        var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == Input.Id);
        if (curso == null)
        {
            return NotFound();
        }

        curso.Titulo = Input.Titulo;
        curso.Descripcion = Input.Descripcion;
        curso.PublicoObjetivo = Input.PublicoObjetivo;
        curso.Orden = Input.Orden;
        curso.Estado = Input.Estado;

        // Manejar cambio de visibilidad
        if (!Input.EsPublico && curso.EsPublico)
        {
            // Cambió de público a privado: generar clave si no existe
            if (string.IsNullOrEmpty(curso.ClaveAcceso))
            {
                curso.ClaveAcceso = GenerateAccessKey();
            }
        }
        else if (Input.EsPublico && !curso.EsPublico)
        {
            // Cambió de privado a público: limpiar claves y tokens
            curso.ClaveAcceso = null;
            curso.TokenAccesoUnico = null;
            curso.TokenExpiracion = null;
        }

        curso.EsPublico = Input.EsPublico;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Curso {CursoId} actualizado por admin", curso.Id);
        
        if (!Input.EsPublico && !string.IsNullOrEmpty(curso.ClaveAcceso))
        {
            TempData["FlashSuccess"] = $"Curso actualizado exitosamente. Clave de acceso: {curso.ClaveAcceso}";
        }
        else
        {
            TempData["FlashSuccess"] = "Curso actualizado exitosamente.";
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
