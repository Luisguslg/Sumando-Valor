namespace SumandoValor.Domain.Entities;

public class AdminAuditEvent
{
    public int Id { get; set; }
    public string ActorUserId { get; set; } = string.Empty;
    public string? ActorEmail { get; set; }
    public string? EntityType { get; set; } // Tipo de entidad afectada (Curso, Taller, Usuario, etc.)
    public string? EntityId { get; set; } // ID de la entidad afectada (puede ser string o int convertido)
    public string TargetUserId { get; set; } = string.Empty; // Mantener para compatibilidad
    public string Action { get; set; } = string.Empty; // Ej: Create, Update, Delete, ToggleActive, MakeModerador, etc.
    public string? OldValuesJson { get; set; } // Valores anteriores (para Update)
    public string? NewValuesJson { get; set; } // Valores nuevos (para Create/Update)
    public string DetailsJson { get; set; } = string.Empty; // Mantener para compatibilidad
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

