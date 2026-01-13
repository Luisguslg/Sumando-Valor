namespace SumandoValor.Domain.Entities;

public class Taller
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Cupos { get; set; }
    public ModalidadTaller Modalidad { get; set; }
    public string? PlataformaDigital { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public int DuracionMin { get; set; }
    public EstatusTaller Estatus { get; set; }
    public string? PublicoObjetivo { get; set; }
    public bool EsPublico { get; set; }
    public string? FacilitadorTexto { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

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
    Cancelado = 3
}
