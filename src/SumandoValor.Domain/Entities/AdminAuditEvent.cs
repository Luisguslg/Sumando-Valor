namespace SumandoValor.Domain.Entities;

public class AdminAuditEvent
{
    public int Id { get; set; }
    public string ActorUserId { get; set; } = string.Empty;
    public string TargetUserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // e.g. ToggleActive, MakeAdmin, RemoveAdmin
    public string DetailsJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

