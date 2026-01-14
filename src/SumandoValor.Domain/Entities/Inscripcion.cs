namespace SumandoValor.Domain.Entities;

public class Inscripcion
{
    public int Id { get; set; }
    public int TallerId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public EstadoInscripcion Estado { get; set; }
    public bool Asistencia { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Taller Taller { get; set; } = null!;
}

public enum EstadoInscripcion
{
    Activa = 1,
    Cancelada = 2
}
