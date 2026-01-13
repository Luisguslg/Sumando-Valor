using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using SumandoValor.Domain.Entities;

namespace SumandoValor.Infrastructure.Data;

public class ApplicationUser : IdentityUser
{
    [Required(ErrorMessage = "Los nombres son requeridos")]
    [StringLength(80, ErrorMessage = "Los nombres no pueden exceder 80 caracteres")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son requeridos")]
    [StringLength(80, ErrorMessage = "Los apellidos no pueden exceder 80 caracteres")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "La cédula es requerida")]
    [StringLength(20, ErrorMessage = "La cédula no puede exceder 20 caracteres")]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "El sexo es requerido")]
    public string Sexo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
    public DateTime FechaNacimiento { get; set; }

    public bool TieneDiscapacidad { get; set; }

    [StringLength(120, ErrorMessage = "La descripción no puede exceder 120 caracteres")]
    public string? DiscapacidadDescripcion { get; set; }

    [Required(ErrorMessage = "El nivel educativo es requerido")]
    public string NivelEducativo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La situación laboral es requerida")]
    public string SituacionLaboral { get; set; } = string.Empty;

    [Required(ErrorMessage = "El canal por el cual conoció es requerido")]
    public string CanalConocio { get; set; } = string.Empty;

    [Required(ErrorMessage = "El estado es requerido")]
    public string Estado { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ciudad es requerida")]
    public string Ciudad { get; set; } = string.Empty;

    [StringLength(25, ErrorMessage = "El teléfono no puede exceder 25 caracteres")]
    public string? Telefono { get; set; }

    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
    public ICollection<Certificado> Certificados { get; set; } = new List<Certificado>();
    public ICollection<EncuestaSatisfaccion> Encuestas { get; set; } = new List<EncuestaSatisfaccion>();
}
