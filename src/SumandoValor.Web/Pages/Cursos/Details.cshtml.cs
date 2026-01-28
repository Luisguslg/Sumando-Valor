using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;
using Microsoft.AspNetCore.Session;

namespace SumandoValor.Web.Pages.Cursos;

public class DetailsModel : PageModel
{
    private readonly AppDbContext _context;

    public DetailsModel(AppDbContext context)
    {
        _context = context;
    }

    public Curso? Curso { get; set; }
    public List<Taller>? Talleres { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, string? token = null)
    {
        Curso = await _context.Cursos
            .FirstOrDefaultAsync(c => c.Id == id && c.Estado == EstatusCurso.Activo);

        if (Curso == null)
        {
            return NotFound();
        }

        // Verificar acceso para cursos no públicos
        if (!Curso.EsPublico)
        {
            // Verificar token único si se proporciona (comparación case-sensitive para seguridad)
            if (!string.IsNullOrEmpty(token) && 
                !string.IsNullOrEmpty(Curso.TokenAccesoUnico) &&
                Curso.TokenAccesoUnico.Equals(token, StringComparison.Ordinal) &&
                (Curso.TokenExpiracion == null || Curso.TokenExpiracion > DateTime.UtcNow))
            {
                // Token válido, guardar en sesión
                HttpContext.Session.SetString($"curso_access_{id}", "granted");
            }
            else
            {
                // Verificar si tiene acceso en sesión
                var hasAccess = HttpContext.Session.GetString($"curso_access_{id}") == "granted";
                
                if (!hasAccess)
                {
                    // Redirigir a página de acceso
                    return RedirectToPage("/Cursos/Access", new { id });
                }
            }
        }

        Talleres = await _context.Talleres
            .Where(t => t.CursoId == id && t.Estatus == EstatusTaller.Abierto)
            .OrderBy(t => t.FechaInicio)
            .ToListAsync();

        // Recalcular cupos disponibles por taller para evitar inconsistencias visuales
        var tallerIds = Talleres.Select(t => t.Id).ToList();
        var inscripcionesActivasPorTaller = await _context.Inscripciones
            .Where(i => tallerIds.Contains(i.TallerId) && i.Estado == EstadoInscripcion.Activa)
            .GroupBy(i => i.TallerId)
            .Select(g => new { TallerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TallerId, x => x.Count);

        foreach (var t in Talleres)
        {
            inscripcionesActivasPorTaller.TryGetValue(t.Id, out var count);
            t.CuposDisponibles = t.CuposMaximos - count;
        }

        return Page();
    }
}
