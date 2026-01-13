using Microsoft.AspNetCore.Identity;
using SumandoValor.Domain.Entities;

namespace SumandoValor.Infrastructure.Data;

public class ApplicationUser : IdentityUser
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public string? Sexo { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public bool Discapacidad { get; set; }
    public string? DescripcionDiscapacidad { get; set; }
    public string? NivelEducativo { get; set; }
    public string? SituacionLaboral { get; set; }
    public string? CanalConocio { get; set; }
    public string? Estado { get; set; }
    public string? Ciudad { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
    public ICollection<Certificado> Certificados { get; set; } = new List<Certificado>();
    public ICollection<EncuestaSatisfaccion> Encuestas { get; set; } = new List<EncuestaSatisfaccion>();
}
