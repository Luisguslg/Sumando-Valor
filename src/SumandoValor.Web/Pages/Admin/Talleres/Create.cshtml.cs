using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Admin.Talleres;

[Authorize(Roles = "Moderador,Admin")]
public class CreateModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(AppDbContext context, ILogger<CreateModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList CursosSelectList { get; set; } = null!;

    public class InputModel
    {
        [Required(ErrorMessage = "El curso es requerido")]
        public int CursoId { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        [Required(ErrorMessage = "La hora de inicio es requerida")]
        public TimeSpan? HoraInicio { get; set; }

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

    public async Task OnGetAsync()
    {
        var cursos = await _context.Cursos
            .Where(c => c.Estado == EstatusCurso.Activo)
            .OrderBy(c => c.Titulo)
            .ToListAsync();

        CursosSelectList = new SelectList(cursos, "Id", "Titulo");
        
        // Valores por defecto
        Input.FechaInicio = DateTime.Today.AddDays(7);
        Input.HoraInicio = new TimeSpan(9, 0, 0);
        Input.Estatus = EstatusTaller.Abierto;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var cursos = await _context.Cursos
            .Where(c => c.Estado == EstatusCurso.Activo)
            .OrderBy(c => c.Titulo)
            .ToListAsync();

        CursosSelectList = new SelectList(cursos, "Id", "Titulo");

        if (Input.Modalidad == ModalidadTaller.Presencial && string.IsNullOrWhiteSpace(Input.Ubicacion))
        {
            ModelState.AddModelError("Input.Ubicacion", "La ubicación es requerida para modalidad Presencial.");
        }
        if (Input.Modalidad != ModalidadTaller.Presencial && string.IsNullOrWhiteSpace(Input.PlataformaDigital))
        {
            ModelState.AddModelError("Input.PlataformaDigital", "La plataforma digital es requerida para modalidad Virtual o Híbrida.");
        }

        // Validar fecha de inicio
        if (Input.FechaInicio.HasValue && Input.FechaInicio.Value.Date < DateTime.Today && Input.Estatus != EstatusTaller.Finalizado)
        {
            ModelState.AddModelError("Input.FechaInicio", "La fecha de inicio no puede ser anterior a hoy, excepto para talleres finalizados.");
        }

        // Validar fecha fin
        if (Input.FechaInicio.HasValue && Input.FechaFin.HasValue && Input.FechaFin.Value.Date < Input.FechaInicio.Value.Date)
        {
            ModelState.AddModelError("Input.FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio.");
        }

        if (!ModelState.IsValid)
        {
            CursosSelectList = new SelectList(cursos, "Id", "Titulo");
            return Page();
        }

        var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == Input.CursoId && c.Estado == EstatusCurso.Activo);
        if (curso == null)
        {
            ModelState.AddModelError("Input.CursoId", "El curso seleccionado no está disponible.");
            return Page();
        }

        var taller = new Taller
        {
            CursoId = Input.CursoId,
            Titulo = Input.Titulo,
            Descripcion = Input.Descripcion,
            FechaInicio = Input.FechaInicio!.Value.Date,
            FechaFin = Input.FechaFin,
            HoraInicio = Input.HoraInicio!.Value,
            Modalidad = Input.Modalidad,
            Ubicacion = Input.Ubicacion,
            PlataformaDigital = Input.PlataformaDigital,
            CuposMaximos = Input.CuposMaximos,
            CuposDisponibles = Input.CuposMaximos,
            Estatus = Input.Estatus,
            FacilitadorTexto = Input.FacilitadorTexto,
            PermiteCertificado = Input.PermiteCertificado,
            RequiereEncuesta = Input.RequiereEncuesta,
            CreatedAt = DateTime.UtcNow
        };

        _context.Talleres.Add(taller);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Taller {TallerId} creado por admin", taller.Id);
        TempData["FlashSuccess"] = "Taller creado exitosamente.";
        return RedirectToPage("/Admin/Talleres");
    }
}
