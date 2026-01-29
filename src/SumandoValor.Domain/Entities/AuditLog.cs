namespace SumandoValor.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? RecordId { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
}
