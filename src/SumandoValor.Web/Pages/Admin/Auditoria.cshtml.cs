using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class AuditoriaModel : PageModel
{
    private readonly AppDbContext _context;
    public int PageSize => 50;

    public AuditoriaModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public string? TableName { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Action { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public List<AuditLog> Logs { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public async Task OnGetAsync()
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(TableName))
        {
            query = query.Where(l => l.TableName == TableName);
        }

        if (!string.IsNullOrWhiteSpace(Action))
        {
            query = query.Where(l => l.Action == Action);
        }

        TotalCount = await query.CountAsync();

        var page = Math.Max(1, PageNumber);
        PageNumber = page;

        Logs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }

    public string GetPageUrl(int page)
    {
        var qs = new List<string> { $"page={page}" };
        if (!string.IsNullOrWhiteSpace(TableName))
            qs.Add($"TableName={Uri.EscapeDataString(TableName)}");
        if (!string.IsNullOrWhiteSpace(Action))
            qs.Add($"Action={Uri.EscapeDataString(Action)}");
        return "/Admin/Auditoria?" + string.Join("&", qs);
    }
}
