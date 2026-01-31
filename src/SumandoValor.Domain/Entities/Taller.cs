namespace SumandoValor.Domain.Entities;

public class Taller
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public ModalidadTaller Modalidad { get; set; }
    public string? Ubicacion { get; set; }
    public string? PlataformaDigital { get; set; }
    public int CuposMaximos { get; set; }
    public int CuposDisponibles { get; set; }
    public EstatusTaller Estatus { get; set; }
    public bool PermiteCertificado { get; set; }
    public bool RequiereEncuesta { get; set; }
    public string? FacilitadorTexto { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Curso Curso { get; set; } = null!;
    public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
    public ICollection<Certificado> Certificados { get; set; } = new List<Certificado>();
    public ICollection<EncuestaSatisfaccion> Encuestas { get; set; } = new List<EncuestaSatisfaccion>();
}

public enum ModalidadTaller
{
    Presencial = 1,
    Virtual = 2,
    Hibrido = 3
}

public enum EstatusTaller
{
    Abierto = 1,
    Cerrado = 2,
    Cancelado = 3,
    Finalizado = 4
}
