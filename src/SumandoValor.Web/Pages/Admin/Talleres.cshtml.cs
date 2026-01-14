using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class TalleresModel : PageModel
{
    private readonly AppDbContext _context;

    public TalleresModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Taller> Talleres { get; set; } = new();

    public async Task OnGetAsync()
    {
        Talleres = await _context.Talleres
            .Include(t => t.Curso)
            .OrderByDescending(t => t.FechaInicio)
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
    }
}
