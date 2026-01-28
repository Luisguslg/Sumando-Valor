namespace SumandoValor.Infrastructure.Services;

public interface IAuditService
{
    Task LogChangeAsync(string actorUserId, string? actorEmail, string entityType, string? entityId, string action, object? oldValues = null, object? newValues = null, string? ipAddress = null);
    Task LogUserActionAsync(string actorUserId, string? actorEmail, string targetUserId, string action, object? details = null, string? ipAddress = null);
}
