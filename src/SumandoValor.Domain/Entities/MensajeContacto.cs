namespace SumandoValor.Domain.Entities;

public class MensajeContacto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public EstadoMensaje Estado { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum EstadoMensaje
{
    Nuevo = 1,
    Leido = 2,
    Archivado = 3
}
