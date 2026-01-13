namespace SumandoValor.Domain.Entities;

public class Certificado
{
    public int Id { get; set; }
    public int TallerId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public EstadoCertificado Estado { get; set; }
    public string? UrlPdf { get; set; }
    public DateTime? IssuedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Taller Taller { get; set; } = null!;
}

public enum EstadoCertificado
{
    Pendiente = 1,
    Aprobado = 2,
    Rechazado = 3
}
