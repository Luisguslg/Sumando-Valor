using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages;

[Authorize(Roles = "Admin")]
public class AdminModel : PageModel
{
    private readonly AppDbContext _context;

    public AdminModel(AppDbContext context)
    {
        _context = context;
    }

    public int TotalCursosActivos { get; set; }
    public int TotalTalleresAbiertos { get; set; }
    public int InscripcionesMes { get; set; }
    public int CuposOcupados { get; set; }

    public async Task OnGetAsync()
    {
        var hoy = DateTime.UtcNow;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        TotalCursosActivos = await _context.Cursos
            .CountAsync(c => c.Estado == EstatusCurso.Activo);

        TotalTalleresAbiertos = await _context.Talleres
            .CountAsync(t => t.Estatus == EstatusTaller.Abierto);

        InscripcionesMes = await _context.Inscripciones
            .CountAsync(i => i.CreatedAt >= inicioMes && i.Estado == EstadoInscripcion.Activa);

        // Calcular porcentaje de cupos ocupados solo para talleres abiertos
        var talleresAbiertos = await _context.Talleres
            .Where(t => t.Estatus == EstatusTaller.Abierto)
            .ToListAsync();

        var totalCuposMaximos = talleresAbiertos.Sum(t => t.CuposMaximos);
        
        if (totalCuposMaximos > 0)
        {
            var inscripcionesActivas = await _context.Inscripciones
                .Where(i => i.Estado == EstadoInscripcion.Activa 
                    && talleresAbiertos.Select(t => t.Id).Contains(i.TallerId))
                .CountAsync();
            
            // Calcular porcentaje con precisión decimal y redondear
            var porcentaje = (double)inscripcionesActivas * 100.0 / totalCuposMaximos;
            CuposOcupados = (int)Math.Round(porcentaje, 0);
        }
        else
        {
            CuposOcupados = 0;
        }
    }
}
