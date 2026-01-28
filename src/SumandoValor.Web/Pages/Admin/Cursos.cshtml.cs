using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Admin;

[Authorize(Roles = "Moderador,Admin")]
public class CursosModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly ILogger<CursosModel> _logger;

    public CursosModel(AppDbContext context, ILogger<CursosModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public List<Curso> Cursos { get; set; } = new();

    public async Task OnGetAsync()
    {
        Cursos = await _context.Cursos
            .Include(c => c.Talleres)
            .OrderBy(c => c.Orden)
            .ThenBy(c => c.Titulo)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostToggleEstadoAsync(int id)
    {
        var curso = await _context.Cursos
            .Include(c => c.Talleres)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (curso == null)
        {
            return NotFound();
        }

        if (curso.Talleres.Count > 0)
        {
            TempData["FlashError"] = "No se puede cambiar el estado de un curso que tiene talleres asociados.";
            return RedirectToPage();
        }

        curso.Estado = curso.Estado == EstatusCurso.Activo ? EstatusCurso.Inactivo : EstatusCurso.Activo;
        await _context.SaveChangesAsync();

        TempData["FlashSuccess"] = $"Curso {(curso.Estado == EstatusCurso.Activo ? "activado" : "desactivado")} exitosamente.";
        return RedirectToPage();
    }
}
