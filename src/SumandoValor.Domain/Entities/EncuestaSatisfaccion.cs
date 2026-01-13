namespace SumandoValor.Domain.Entities;

public class EncuestaSatisfaccion
{
    public int Id { get; set; }
    public int TallerId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public decimal? ScorePromedio { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Taller Taller { get; set; } = null!;
}
