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
    public string? HomePillarsImageUrl { get; set; }
    public string HomePillarsImageAlt { get; set; } = "Pilares de formación";

    public async Task OnGetAsync()
    {
        var pillars = await _context.SiteImages.AsNoTracking().FirstOrDefaultAsync(x => x.Key == "HomePillars");
        if (pillars != null)
        {
            HomePillarsImageUrl = Url.Content("~/uploads/site/" + pillars.FileName);
            HomePillarsImageAlt = pillars.AltText;
        }

        CarouselItems = await _context.CarouselItems
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Take(10)
            .ToListAsync();

        // Solo mostrar cursos públicos en el home
        CursosDestacados = await _context.Cursos
            .Where(c => c.Estado == EstatusCurso.Activo && c.EsPublico)
            .OrderBy(c => c.Orden)
            .ThenBy(c => c.Titulo)
            .Take(3)
            .ToListAsync();

        var today = DateTime.Today;
        // Solo mostrar talleres de cursos públicos
        TalleresProximos = await _context.Talleres
            .Include(t => t.Curso)
            .Where(t => t.Estatus == EstatusTaller.Abierto && 
                       t.FechaInicio.Date >= today &&
                       t.Curso.EsPublico)
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
