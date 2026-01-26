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

        // Sólo contar inscripciones activas que pertenezcan a talleres que están abiertos.
        var cuposMaximos = await _context.Talleres
            .Where(t => t.Estatus == EstatusTaller.Abierto)
            .SumAsync(t => (int?)t.CuposMaximos) ?? 0;

        var inscripcionesActivas = await (
            from i in _context.Inscripciones
            join t in _context.Talleres on i.TallerId equals t.Id
            where i.Estado == EstadoInscripcion.Activa && t.Estatus == EstatusTaller.Abierto
            select i
        ).CountAsync();
        // Usar división en punto flotante y redondear para mostrar porcentaje razonable
        CuposOcupados = cuposMaximos > 0 ? (int)Math.Round((double)inscripcionesActivas * 100.0 / cuposMaximos) : 0;
    }
}
