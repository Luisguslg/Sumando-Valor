using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Admin.Talleres;

[Authorize(Roles = "Moderador,Admin")]
public class EditModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly ILogger<EditModel> _logger;

    public EditModel(AppDbContext context, ILogger<EditModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Taller? Taller { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public DateTime FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        [Required(ErrorMessage = "La hora de inicio es requerida")]
        public TimeSpan HoraInicio { get; set; }

        [Required(ErrorMessage = "La modalidad es requerida")]
        public ModalidadTaller Modalidad { get; set; }

        [StringLength(300, ErrorMessage = "La ubicación no puede exceder 300 caracteres")]
        public string? Ubicacion { get; set; }

        [StringLength(200, ErrorMessage = "La plataforma digital no puede exceder 200 caracteres")]
        public string? PlataformaDigital { get; set; }

        [Required(ErrorMessage = "Los cupos máximos son requeridos")]
        [Range(1, int.MaxValue, ErrorMessage = "Los cupos deben ser mayor a 0")]
        public int CuposMaximos { get; set; }

        [Required]
        public EstatusTaller Estatus { get; set; }

        [StringLength(200, ErrorMessage = "El facilitador no puede exceder 200 caracteres")]
        public string? FacilitadorTexto { get; set; }

        public bool PermiteCertificado { get; set; }

        public bool RequiereEncuesta { get; set; }
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

        Input = new InputModel
        {
            Id = Taller.Id,
            Titulo = Taller.Titulo,
            Descripcion = Taller.Descripcion,
            FechaInicio = Taller.FechaInicio,
            FechaFin = Taller.FechaFin,
            HoraInicio = Taller.HoraInicio,
            Modalidad = Taller.Modalidad,
            PlataformaDigital = Taller.PlataformaDigital,
            CuposMaximos = Taller.CuposMaximos,
            Estatus = Taller.Estatus,
            FacilitadorTexto = Taller.FacilitadorTexto,
            Ubicacion = Taller.Ubicacion,
            PermiteCertificado = Taller.PermiteCertificado,
            RequiereEncuesta = Taller.RequiereEncuesta
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.Modalidad == ModalidadTaller.Presencial && string.IsNullOrWhiteSpace(Input.Ubicacion))
        {
            ModelState.AddModelError("Input.Ubicacion", "La ubicación es requerida para modalidad Presencial.");
        }
        if (Input.Modalidad != ModalidadTaller.Presencial && string.IsNullOrWhiteSpace(Input.PlataformaDigital))
        {
            ModelState.AddModelError("Input.PlataformaDigital", "La plataforma digital es requerida para modalidad Virtual o Híbrida.");
        }

        // Validar fecha de inicio
        if (Input.FechaInicio < DateTime.Today && Input.Estatus != EstatusTaller.Finalizado)
        {
            ModelState.AddModelError("Input.FechaInicio", "La fecha de inicio no puede ser anterior a hoy, excepto para talleres finalizados.");
        }

        // Validar fecha fin
        if (Input.FechaFin.HasValue && Input.FechaFin.Value < Input.FechaInicio)
        {
            ModelState.AddModelError("Input.FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio.");
        }

        if (!ModelState.IsValid)
        {
            Taller = await _context.Talleres
                .Include(t => t.Curso)
                .FirstOrDefaultAsync(t => t.Id == Input.Id);
            return Page();
        }

        var taller = await _context.Talleres
            .FirstOrDefaultAsync(t => t.Id == Input.Id);

        if (taller == null)
        {
            return NotFound();
        }

        var inscripcionesActivas = await _context.Inscripciones
            .CountAsync(i => i.TallerId == taller.Id && i.Estado == EstadoInscripcion.Activa);
        if (Input.CuposMaximos < inscripcionesActivas)
        {
            ModelState.AddModelError("Input.CuposMaximos", $"No se pueden reducir los cupos por debajo de las inscripciones activas ({inscripcionesActivas}).");
            Taller = await _context.Talleres
                .Include(t => t.Curso)
                .FirstOrDefaultAsync(t => t.Id == Input.Id);
            return Page();
        }

        taller.Titulo = Input.Titulo;
        taller.Descripcion = Input.Descripcion;
        taller.FechaInicio = Input.FechaInicio;
        taller.FechaFin = Input.FechaFin;
        taller.HoraInicio = Input.HoraInicio;
        taller.Modalidad = Input.Modalidad;
        taller.PlataformaDigital = Input.PlataformaDigital;
        taller.CuposMaximos = Input.CuposMaximos;
        // Recalcular SIEMPRE por taller (evita drift / reinicios raros)
        taller.CuposDisponibles = Input.CuposMaximos - inscripcionesActivas;
        taller.Estatus = Input.Estatus;
        taller.FacilitadorTexto = Input.FacilitadorTexto;
        taller.Ubicacion = Input.Ubicacion;
        taller.PermiteCertificado = Input.PermiteCertificado;
        taller.RequiereEncuesta = Input.RequiereEncuesta;
        taller.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Taller {TallerId} actualizado por admin", taller.Id);
        TempData["FlashSuccess"] = "Taller actualizado exitosamente.";
        return RedirectToPage("/Admin/Talleres");
    }
}
