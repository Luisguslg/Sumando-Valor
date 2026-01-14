using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;
using System.Text;

namespace SumandoValor.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class InscripcionesModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly ILogger<InscripcionesModel> _logger;

    public InscripcionesModel(AppDbContext context, ILogger<InscripcionesModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public List<InscripcionViewModel> Inscripciones { get; set; } = new();
    public List<Taller> TalleresDisponibles { get; set; } = new();
    public int? TallerIdFiltro { get; set; }

    public class InscripcionViewModel
    {
        public Inscripcion Inscripcion { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }

    public async Task OnGetAsync(int? tallerId = null)
    {
        TallerIdFiltro = tallerId;

        var query = _context.Inscripciones
            .Include(i => i.Taller)
                .ThenInclude(t => t.Curso)
            .AsQueryable();

        if (tallerId.HasValue)
        {
            query = query.Where(i => i.TallerId == tallerId.Value);
        }

        var inscripciones = await query
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        var userIds = inscripciones.Select(i => i.UserId).Distinct().ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        Inscripciones = inscripciones.Select(i => new InscripcionViewModel
        {
            Inscripcion = i,
            User = users[i.UserId]
        }).ToList();

        TalleresDisponibles = await _context.Talleres
            .Include(t => t.Curso)
            .OrderBy(t => t.Titulo)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostToggleAsistenciaAsync(int id)
    {
        var inscripcion = await _context.Inscripciones
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inscripcion == null)
        {
            return NotFound();
        }

        inscripcion.Asistencia = !inscripcion.Asistencia;
        await _context.SaveChangesAsync();

        TempData["FlashSuccess"] = $"Asistencia {(inscripcion.Asistencia ? "marcada" : "desmarcada")} exitosamente.";
        return RedirectToPage(new { tallerId = TallerIdFiltro });
    }

    public async Task<IActionResult> OnPostCancelarAsync(int id)
    {
        var inscripcion = await _context.Inscripciones
            .Include(i => i.Taller)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inscripcion == null)
        {
            return NotFound();
        }

        if (inscripcion.Estado == EstadoInscripcion.Cancelada)
        {
            TempData["FlashInfo"] = "La inscripción ya está cancelada.";
            return RedirectToPage(new { tallerId = TallerIdFiltro });
        }

        inscripcion.Estado = EstadoInscripcion.Cancelada;
        
        // Recalcular cupos disponibles dinámicamente
        var inscripcionesActivas = await _context.Inscripciones
            .CountAsync(i => i.TallerId == inscripcion.TallerId && i.Estado == EstadoInscripcion.Activa);
        
        inscripcion.Taller.CuposDisponibles = inscripcion.Taller.CuposMaximos - inscripcionesActivas;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Inscripción {InscripcionId} cancelada por admin", id);
        TempData["FlashSuccess"] = "Inscripción cancelada exitosamente. Cupo liberado.";
        return RedirectToPage(new { tallerId = TallerIdFiltro });
    }

    public async Task<IActionResult> OnPostExportCsvAsync()
    {
        var inscripciones = await _context.Inscripciones
            .Include(i => i.Taller)
                .ThenInclude(t => t.Curso)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        var userIds = inscripciones.Select(i => i.UserId).Distinct().ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var csv = new StringBuilder();
        csv.AppendLine("Taller,Curso,Usuario,Email,Fecha Inscripción,Estado,Asistencia");

        foreach (var ins in inscripciones)
        {
            var user = users[ins.UserId];
            csv.AppendLine($"{EscapeCsv(ins.Taller.Titulo)},{EscapeCsv(ins.Taller.Curso.Titulo)},{EscapeCsv($"{user.Nombres} {user.Apellidos}")},{EscapeCsv(user.Email ?? "")},{ins.CreatedAt:yyyy-MM-dd HH:mm},{ins.Estado},{(ins.Asistencia ? "Sí" : "No")}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"inscripciones_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
