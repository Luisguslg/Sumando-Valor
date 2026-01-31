using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;
using SumandoValor.Infrastructure.Services;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;

namespace SumandoValor.Web.Pages.Admin;

[Authorize(Roles = "Moderador,Admin")]
public class InscripcionesModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly ILogger<InscripcionesModel> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public InscripcionesModel(AppDbContext context, ILogger<InscripcionesModel> logger, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public List<InscripcionViewModel> Inscripciones { get; set; } = new();
    public List<Taller> TalleresDisponibles { get; set; } = new();
    public List<ApplicationUser> UsuariosDisponibles { get; set; } = new();
    [BindProperty(SupportsGet = true, Name = "tallerId")]
    public int? TallerIdFiltro { get; set; }

    [BindProperty]
    public List<int> SelectedInscripcionIds { get; set; } = new();

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

        UsuariosDisponibles = await _context.Users
            .Where(u => u.EmailConfirmed)
            .OrderBy(u => u.Nombres)
            .ThenBy(u => u.Apellidos)
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

    public async Task<IActionResult> OnPostUpdateAttendanceBatchAsync(bool attended)
    {
        if (SelectedInscripcionIds == null || !SelectedInscripcionIds.Any())
        {
            TempData["FlashInfo"] = "No se seleccionaron inscripciones.";
            return RedirectToPage(new { tallerId = TallerIdFiltro });
        }

        var inscripciones = await _context.Inscripciones
            .Where(i => SelectedInscripcionIds.Contains(i.Id))
            .ToListAsync();

        foreach (var ins in inscripciones)
        {
            ins.Asistencia = attended;
        }

        await _context.SaveChangesAsync();
        TempData["FlashSuccess"] = $"Asistencia actualizada para {inscripciones.Count} inscripciones.";
        return RedirectToPage(new { tallerId = TallerIdFiltro });
    }

    public async Task<IActionResult> OnPostSendEmailBatchAsync(string subject, string message)
    {
        if (SelectedInscripcionIds == null || !SelectedInscripcionIds.Any())
        {
            TempData["FlashInfo"] = "No se seleccionaron destinatarios.";
            return RedirectToPage(new { tallerId = TallerIdFiltro });
        }

        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
        {
            TempData["FlashError"] = "Asunto y mensaje son requeridos.";
            return RedirectToPage(new { tallerId = TallerIdFiltro });
        }

        var userIds = await _context.Inscripciones
            .Where(i => SelectedInscripcionIds.Contains(i.Id))
            .Select(i => i.UserId)
            .Distinct()
            .ToListAsync();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        int successCount = 0;
        var emailService = _httpContextAccessor.HttpContext?.RequestServices.GetService<IEmailService>();
        
        if (emailService != null)
        {
            var htmlBody = EmailTemplates.GenericMessageHtml(subject, message);
            
            foreach (var user in users)
            {
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    try
                    {
                        await emailService.SendEmailAsync(user.Email, subject, htmlBody, isHtml: true);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error enviando correo masivo a {Email}", user.Email);
                    }
                }
            }
        }

        TempData["FlashSuccess"] = $"Correo enviado a {successCount} usuarios.";
        return RedirectToPage(new { tallerId = TallerIdFiltro });
    }

    public async Task<IActionResult> OnPostExportCsvAsync()
    {
        var query = _context.Inscripciones
            .Include(i => i.Taller)
                .ThenInclude(t => t.Curso)
            .AsQueryable();
        if (TallerIdFiltro.HasValue)
            query = query.Where(i => i.TallerId == TallerIdFiltro.Value);
        var inscripciones = await query
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

        var csvContent = csv.ToString();
        var preamble = Encoding.UTF8.GetPreamble();
        var contentBytes = Encoding.UTF8.GetBytes(csvContent);
        var bytes = new byte[preamble.Length + contentBytes.Length];
        Buffer.BlockCopy(preamble, 0, bytes, 0, preamble.Length);
        Buffer.BlockCopy(contentBytes, 0, bytes, preamble.Length, contentBytes.Length);
        return File(bytes, "text/csv; charset=utf-8", $"inscripciones_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
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

    public async Task<IActionResult> OnPostInscribirUsuarioAsync(int tallerId, string userId)
    {
        // Validación de entrada
        if (tallerId <= 0)
        {
            TempData["FlashError"] = "Taller inválido.";
            return RedirectToPage();
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            TempData["FlashError"] = "Debe seleccionar un usuario.";
            return RedirectToPage();
        }

        var taller = await _context.Talleres
            .Include(t => t.Curso)
            .FirstOrDefaultAsync(t => t.Id == tallerId);

        if (taller == null)
        {
            TempData["FlashError"] = "Taller no encontrado.";
            return RedirectToPage();
        }

        if (taller.Estatus != EstatusTaller.Abierto)
        {
            TempData["FlashError"] = "El taller no está abierto para inscripciones.";
            return RedirectToPage();
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            TempData["FlashError"] = "Usuario no encontrado.";
            return RedirectToPage();
        }

        // Usar transacción para evitar race conditions
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            // Releer taller dentro de la transacción
            var tallerActualizado = await _context.Talleres
                .FirstOrDefaultAsync(t => t.Id == tallerId);

            if (tallerActualizado == null)
            {
                await transaction.RollbackAsync();
                TempData["FlashError"] = "Taller no encontrado.";
                return RedirectToPage();
            }

            if (tallerActualizado.Estatus != EstatusTaller.Abierto)
            {
                await transaction.RollbackAsync();
                TempData["FlashError"] = "El taller no está abierto para inscripciones.";
                return RedirectToPage();
            }

            // Verificar si ya está inscrito (dentro de la transacción)
            // Verificar tanto inscripciones activas como canceladas para evitar duplicados
            var existeInscripcion = await _context.Inscripciones
                .AnyAsync(i => i.TallerId == tallerId && i.UserId == userId);

            if (existeInscripcion)
            {
                // Verificar si es activa o cancelada
                var inscripcionExistente = await _context.Inscripciones
                    .FirstOrDefaultAsync(i => i.TallerId == tallerId && i.UserId == userId);

                if (inscripcionExistente != null && inscripcionExistente.Estado == EstadoInscripcion.Activa)
                {
                    await transaction.RollbackAsync();
                    TempData["FlashError"] = "El usuario ya está inscrito en este taller.";
                    return RedirectToPage();
                }
                // Si está cancelada, podemos reactivarla o crear una nueva según la lógica de negocio
                // Por ahora, permitimos crear una nueva inscripción si la anterior estaba cancelada
            }

            // Verificar cupos disponibles (dentro de la transacción)
            var inscripcionesActivas = await _context.Inscripciones
                .CountAsync(i => i.TallerId == tallerId && i.Estado == EstadoInscripcion.Activa);

            if (inscripcionesActivas >= tallerActualizado.CuposMaximos)
            {
                await transaction.RollbackAsync();
                TempData["FlashError"] = "No hay cupos disponibles para este taller.";
                return RedirectToPage();
            }

            // Crear inscripción
            var inscripcion = new Inscripcion
            {
                TallerId = tallerId,
                UserId = userId,
                Estado = EstadoInscripcion.Activa,
                CreatedAt = DateTime.UtcNow
            };

            _context.Inscripciones.Add(inscripcion);
            
            // Actualizar cupos disponibles
            tallerActualizado.CuposDisponibles = tallerActualizado.CuposMaximos - inscripcionesActivas - 1;
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx) when (dbEx.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 2601)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning("Intento de inscripción duplicada: Usuario {UserId} en taller {TallerId}", userId, tallerId);
            TempData["FlashError"] = "El usuario ya está inscrito en este taller.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al inscribir usuario {UserId} en taller {TallerId}", userId, tallerId);
            TempData["FlashError"] = "Error al procesar la inscripción. Por favor intenta nuevamente.";
            return RedirectToPage();
        }

        _logger.LogInformation("Usuario {UserId} inscrito en taller {TallerId} por admin", userId, tallerId);
        TempData["FlashSuccess"] = $"Usuario {user.Nombres} {user.Apellidos} inscrito exitosamente en {taller.Titulo}.";
        
        return RedirectToPage(new { tallerId = TallerIdFiltro });
    }
}
