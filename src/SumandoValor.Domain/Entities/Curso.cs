namespace SumandoValor.Domain.Entities;

public class Curso
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? PublicoObjetivo { get; set; }
    public bool EsPublico { get; set; }
    public string? ClaveAcceso { get; set; }
    public string? TokenAccesoUnico { get; set; }
    public DateTime? TokenExpiracion { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public EstatusCurso Estado { get; set; } = EstatusCurso.Activo;
    public int? Orden { get; set; }

    public ICollection<Taller> Talleres { get; set; } = new List<Taller>();
}

public enum EstatusCurso
{
    Activo = 1,
    Inactivo = 2
}
