using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Data;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Talleres;

public class DetailsModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<DetailsModel> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public Taller? Taller { get; set; }
    public bool EstaInscrito { get; set; }
    public InscripcionUiState UiState { get; set; } = InscripcionUiState.Unknown;
    public string? UiBannerText { get; set; }
    public string UiBannerClass { get; set; } = "alert-info";

    public enum InscripcionUiState
    {
        Unknown = 0,
        MustLogin = 1,
        NotBeneficiario = 2,
        AlreadyEnrolled = 3,
        NotOpen = 4,
        Full = 5,
        CanEnroll = 6
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Taller = await _context.Talleres
            .Include(t => t.Curso)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (Taller == null)
        {
            return NotFound();
        }

        if (User.Identity?.IsAuthenticated != true)
        {
            UiState = InscripcionUiState.MustLogin;
            UiBannerText = "Debes iniciar sesión para inscribirte.";
            UiBannerClass = "alert-info";
        }
        else
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                EstaInscrito = await _context.Inscripciones
                    .AnyAsync(i => i.TallerId == id && i.UserId == user.Id && i.Estado == EstadoInscripcion.Activa);
            }
        }

        // Cupos disponibles SIEMPRE por taller (derivado)
        var inscripcionesActivas = await _context.Inscripciones
            .CountAsync(i => i.TallerId == id && i.Estado == EstadoInscripcion.Activa);
        Taller.CuposDisponibles = Taller.CuposMaximos - inscripcionesActivas;

        // Determinar estado UI (mutuamente excluyente)
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            var esBeneficiario = user != null && await _userManager.IsInRoleAsync(user, "Beneficiario");

            if (!esBeneficiario)
            {
                UiState = InscripcionUiState.NotBeneficiario;
                UiBannerText = "Solo los beneficiarios pueden inscribirse en talleres.";
                UiBannerClass = "alert-warning";
            }
            else if (EstaInscrito)
            {
                UiState = InscripcionUiState.AlreadyEnrolled;
                UiBannerText = "Ya estás inscrito en este taller.";
                UiBannerClass = "alert-success";
            }
            else if (Taller.Estatus != EstatusTaller.Abierto)
            {
                UiState = InscripcionUiState.NotOpen;
                UiBannerText = Taller.Estatus switch
                {
                    EstatusTaller.Cerrado => "Este taller está cerrado para inscripciones.",
                    EstatusTaller.Cancelado => "Este taller fue cancelado.",
                    EstatusTaller.Finalizado => "Este taller ya finalizó.",
                    _ => "Este taller no está abierto para inscripciones."
                };
                UiBannerClass = "alert-warning";
            }
            else if (Taller.CuposDisponibles <= 0)
            {
                UiState = InscripcionUiState.Full;
                UiBannerText = "No hay cupos disponibles.";
                UiBannerClass = "alert-warning";
            }
            else
            {
                UiState = InscripcionUiState.CanEnroll;
                UiBannerText = null;
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostInscribirseAsync(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var esBeneficiario = await _userManager.IsInRoleAsync(user, "Beneficiario");
        if (!esBeneficiario)
        {
            TempData["FlashError"] = "Solo los beneficiarios pueden inscribirse en talleres.";
            return RedirectToPage(new { id });
        }

        Taller = await _context.Talleres
            .Include(t => t.Curso)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (Taller == null)
        {
            TempData["FlashError"] = "Taller no encontrado.";
            return RedirectToPage("/Cursos");
        }

        if (Taller.Estatus != EstatusTaller.Abierto)
        {
            TempData["FlashError"] = "Este taller no está abierto para inscripciones.";
            return RedirectToPage(new { id });
        }

        // Usar transacción SERIALIZABLE para evitar sobre-inscripción por concurrencia
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            // Releer taller dentro de la transacción
            var tallerActualizado = await _context.Talleres
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tallerActualizado == null)
            {
                await transaction.RollbackAsync();
                TempData["FlashError"] = "Taller no encontrado.";
                return RedirectToPage("/Cursos");
            }

            if (tallerActualizado.Estatus != EstatusTaller.Abierto)
            {
                await transaction.RollbackAsync();
                TempData["FlashError"] = "Este taller no está abierto para inscripciones.";
                return RedirectToPage(new { id });
            }

            // Buscar inscripción existente (por índice único TallerId+UserId)
            var existente = await _context.Inscripciones
                .FirstOrDefaultAsync(i => i.TallerId == id && i.UserId == user.Id);

            if (existente is not null && existente.Estado == EstadoInscripcion.Activa)
            {
                await transaction.RollbackAsync();
                TempData["FlashInfo"] = "Ya estás inscrito en este taller.";
                return RedirectToPage(new { id });
            }

            var inscripcionesActivas = await _context.Inscripciones
                .CountAsync(i => i.TallerId == id && i.Estado == EstadoInscripcion.Activa);

            var cuposDisponibles = tallerActualizado.CuposMaximos - inscripcionesActivas;
            if (cuposDisponibles <= 0)
            {
                await transaction.RollbackAsync();
                TempData["FlashError"] = "No hay cupos disponibles para este taller.";
                return RedirectToPage(new { id });
            }

            if (existente is null)
            {
                _context.Inscripciones.Add(new Inscripcion
                {
                    TallerId = id,
                    UserId = user.Id,
                    Estado = EstadoInscripcion.Activa,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                // Re-activar inscripción cancelada (evita violación del índice único)
                existente.Estado = EstadoInscripcion.Activa;
                existente.Asistencia = false;
                existente.CreatedAt = DateTime.UtcNow;
            }

            // Mantener CuposDisponibles consistente aunque se calcule dinámico
            tallerActualizado.CuposDisponibles = cuposDisponibles - 1;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Inscripción exitosa. userId={UserId}, tallerId={TallerId}, modalidad={Modalidad}, estatus={Estatus}, cuposMax={CuposMaximos}",
                user.Id, id, tallerActualizado.Modalidad, tallerActualizado.Estatus, tallerActualizado.CuposMaximos);

            TempData["FlashSuccess"] = "Inscripción realizada con éxito.";
            return RedirectToPage("/Profile/Talleres");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            var modelStateErrors = string.Join(" | ",
                ModelState
                    .Where(kvp => kvp.Value?.Errors.Count > 0)
                    .Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value!.Errors.Select(e => e.ErrorMessage))}"));

            _logger.LogError(
                ex,
                "Error al inscribir. userId={UserId}, tallerId={TallerId}, estatus={Estatus}, cuposMax={CuposMaximos}, cuposDispPersistido={CuposDisponiblesPersistido}, modelState={ModelStateErrors}",
                user.Id, id, Taller.Estatus, Taller.CuposMaximos, Taller.CuposDisponibles, modelStateErrors);

            TempData["FlashError"] = "Ocurrió un error al procesar tu inscripción. Por favor intenta nuevamente.";
            return RedirectToPage(new { id });
        }
    }
}
