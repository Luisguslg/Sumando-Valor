using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;
using System.Text.Json;

namespace SumandoValor.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class AuditoriaModel : PageModel
{
    private const int PageSize = 50;
    private readonly AppDbContext _context;

    public AuditoriaModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public string? EntityType { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Action { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true, Name = "page")]
    public int PageNumber { get; set; } = 1;

    public List<AuditEventViewModel> Events { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public async Task OnGetAsync()
    {
        var query = _context.AdminAuditEvents.AsQueryable();

        // Filtros
        if (!string.IsNullOrWhiteSpace(EntityType))
        {
            query = query.Where(e => e.EntityType == EntityType);
        }

        if (!string.IsNullOrWhiteSpace(Action))
        {
            query = query.Where(e => e.Action == Action);
        }

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var searchTerm = Search.Trim();
            query = query.Where(e =>
                (e.ActorEmail != null && e.ActorEmail.Contains(searchTerm)) ||
                (e.EntityType != null && e.EntityType.Contains(searchTerm)) ||
                (e.Action != null && e.Action.Contains(searchTerm)) ||
                (e.EntityId != null && e.EntityId.Contains(searchTerm)));
        }

        TotalCount = await query.CountAsync();

        var page = Math.Max(1, PageNumber);
        PageNumber = page;

        var events = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        // Obtener información de usuarios para mostrar nombres
        var actorUserIds = events.Select(e => e.ActorUserId).Distinct().ToList();
        var users = await _context.Users
            .Where(u => actorUserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => $"{u.Nombres} {u.Apellidos}".Trim());

        Events = events.Select(e => new AuditEventViewModel
        {
            Id = e.Id,
            ActorUserId = e.ActorUserId,
            ActorName = users.TryGetValue(e.ActorUserId, out var name) ? name : e.ActorEmail ?? "Sistema",
            ActorEmail = e.ActorEmail,
            EntityType = e.EntityType ?? "Usuario",
            EntityId = e.EntityId ?? e.TargetUserId,
            Action = e.Action,
            OldValues = !string.IsNullOrEmpty(e.OldValuesJson) ? TryParseJson(e.OldValuesJson) : null,
            NewValues = !string.IsNullOrEmpty(e.NewValuesJson) ? TryParseJson(e.NewValuesJson) : null,
            Details = !string.IsNullOrEmpty(e.DetailsJson) ? TryParseJson(e.DetailsJson) : null,
            IpAddress = e.IpAddress,
            CreatedAt = e.CreatedAt
        }).ToList();
    }

    private static Dictionary<string, object>? TryParseJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        }
        catch
        {
            return null;
        }
    }

    public string PageUrl(int page)
    {
        var qs = new Dictionary<string, string?>
        {
            ["page"] = page.ToString(),
            ["EntityType"] = EntityType,
            ["Action"] = Action,
            ["Search"] = Search
        };

        var parts = qs.Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");
        return "/Admin/Auditoria" + (parts.Any() ? "?" + string.Join("&", parts) : "");
    }

    public class AuditEventViewModel
    {
        public int Id { get; set; }
        public string ActorUserId { get; set; } = "";
        public string ActorName { get; set; } = "";
        public string? ActorEmail { get; set; }
        public string EntityType { get; set; } = "";
        public string? EntityId { get; set; }
        public string Action { get; set; } = "";
        public Dictionary<string, object>? OldValues { get; set; }
        public Dictionary<string, object>? NewValues { get; set; }
        public Dictionary<string, object>? Details { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
