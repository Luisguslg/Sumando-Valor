using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Curso> CursosDestacados { get; set; } = new();
    public List<Taller> TalleresProximos { get; set; } = new();
    public List<CarouselItem> CarouselItems { get; set; } = new();

    public async Task OnGetAsync()
    {
        CarouselItems = await _context.CarouselItems
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Take(10)
            .ToListAsync();

        CursosDestacados = await _context.Cursos
            .Where(c => c.Estado == EstatusCurso.Activo)
            .OrderBy(c => c.Orden)
            .ThenBy(c => c.Titulo)
            .Take(3)
            .ToListAsync();

        var today = DateTime.Today;
        TalleresProximos = await _context.Talleres
            .Include(t => t.Curso)
            .Where(t => t.Estatus == EstatusTaller.Abierto && t.FechaInicio.Date >= today)
            .OrderBy(t => t.FechaInicio)
            .ThenBy(t => t.HoraInicio)
            .Take(3)
            .ToListAsync();

        // Cupos disponibles derivado por taller (visual)
        var tallerIds = TalleresProximos.Select(t => t.Id).ToList();
        var inscripcionesActivas = await _context.Inscripciones
            .Where(i => tallerIds.Contains(i.TallerId) && i.Estado == EstadoInscripcion.Activa)
            .GroupBy(i => i.TallerId)
            .Select(g => new { TallerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TallerId, x => x.Count);

        foreach (var t in TalleresProximos)
        {
            inscripcionesActivas.TryGetValue(t.Id, out var count);
            t.CuposDisponibles = t.CuposMaximos - count;
        }
    }
}
