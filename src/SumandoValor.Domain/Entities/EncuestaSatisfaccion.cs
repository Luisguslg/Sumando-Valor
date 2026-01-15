namespace SumandoValor.Domain.Entities;

public class EncuestaSatisfaccion
{
    public int Id { get; set; }
    public int TallerId { get; set; }
    public string UserId { get; set; } = string.Empty;

    // Encuesta simple (v1)
    public int Rating1_5 { get; set; }
    public string? Comentario { get; set; }

    // Campo de compatibilidad / extensibilidad (v2+)
    public string PayloadJson { get; set; } = string.Empty;
    public decimal? ScorePromedio { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Taller Taller { get; set; } = null!;
}
