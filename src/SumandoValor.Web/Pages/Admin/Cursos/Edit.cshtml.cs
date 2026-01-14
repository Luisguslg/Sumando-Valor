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
        curso.EsPublico = Input.EsPublico;
        curso.Orden = Input.Orden;
        curso.Estado = Input.Estado;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Curso {CursoId} actualizado por admin", curso.Id);
        TempData["FlashSuccess"] = "Curso actualizado exitosamente.";
        return RedirectToPage("/Admin/Cursos");
    }
}
