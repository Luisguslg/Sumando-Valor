using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;
using System.Text.Json;

namespace SumandoValor.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(AppDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogChangeAsync(string actorUserId, string? actorEmail, string entityType, string? entityId, string action, object? oldValues = null, object? newValues = null, string? ipAddress = null)
    {
        try
        {
            var oldJson = oldValues != null ? JsonSerializer.Serialize(oldValues) : null;
            var newJson = newValues != null ? JsonSerializer.Serialize(newValues) : null;
            var detailsJson = JsonSerializer.Serialize(new { entityType, entityId, action });

            _context.AdminAuditEvents.Add(new AdminAuditEvent
            {
                ActorUserId = actorUserId,
                ActorEmail = actorEmail,
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                OldValuesJson = oldJson,
                NewValuesJson = newJson,
                DetailsJson = detailsJson,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo registrar auditoría (EntityType={EntityType}, Action={Action})", entityType, action);
        }
    }

    public async Task LogUserActionAsync(string actorUserId, string? actorEmail, string targetUserId, string action, object? details = null, string? ipAddress = null)
    {
        try
        {
            var detailsJson = details != null ? JsonSerializer.Serialize(details) : string.Empty;

            _context.AdminAuditEvents.Add(new AdminAuditEvent
            {
                ActorUserId = actorUserId,
                ActorEmail = actorEmail,
                TargetUserId = targetUserId,
                EntityType = "Usuario",
                EntityId = targetUserId,
                Action = action,
                DetailsJson = detailsJson,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo registrar auditoría de usuario (Action={Action})", action);
        }
    }
}
