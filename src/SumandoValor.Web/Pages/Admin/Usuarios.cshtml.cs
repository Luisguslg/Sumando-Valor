using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class UsuariosModel : PageModel
{
    private const int PageSize = 20;
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UsuariosModel> _logger;

    public UsuariosModel(AppDbContext context, UserManager<ApplicationUser> userManager, ILogger<UsuariosModel> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; } // active | inactive

    [BindProperty(SupportsGet = true)]
    public string? Role { get; set; } // Admin | Beneficiario

    [BindProperty(SupportsGet = true, Name = "page")]
    public int PageNumber { get; set; } = 1;

    public List<Row> Rows { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public async Task OnGetAsync()
    {
        var q = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim();
            q = q.Where(u =>
                (u.Nombres != null && u.Nombres.Contains(s)) ||
                (u.Apellidos != null && u.Apellidos.Contains(s)) ||
                (u.Email != null && u.Email.Contains(s)) ||
                (u.Cedula != null && u.Cedula.Contains(s)));
        }

        if (Status == "active")
        {
            q = q.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow);
        }
        else if (Status == "inactive")
        {
            q = q.Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow);
        }

        // Role filter (server-side): join to AspNetUserRoles
        if (!string.IsNullOrWhiteSpace(Role))
        {
            var roleId = await _context.Roles.Where(r => r.Name == Role).Select(r => r.Id).FirstOrDefaultAsync();
            if (!string.IsNullOrWhiteSpace(roleId))
            {
                q = from u in q
                    join ur in _context.UserRoles on u.Id equals ur.UserId
                    where ur.RoleId == roleId
                    select u;
            }
            else
            {
                q = q.Where(_ => false);
            }
        }

        TotalCount = await q.CountAsync();

        var page = Math.Max(1, PageNumber);
        PageNumber = page;

        var users = await q
            .OrderBy(u => u.Nombres)
            .ThenBy(u => u.Apellidos)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        Rows = new List<Row>(users.Count);
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            Rows.Add(new Row
            {
                UserId = u.Id,
                FullName = $"{u.Nombres} {u.Apellidos}".Trim(),
                Email = u.Email ?? "",
                Cedula = u.Cedula ?? "",
                IsActive = u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow,
                Roles = roles.ToList(),
                IsAdmin = roles.Contains("Admin")
            });
        }
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(string id)
    {
        var actorIsSuperAdmin = User.IsInRole("SuperAdmin");
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["FlashError"] = "Usuario no encontrado.";
            return RedirectToPage();
        }

        var isActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;
        if (isActive)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin") || roles.Contains("SuperAdmin"))
            {
                if (!actorIsSuperAdmin)
                {
                    TempData["FlashError"] = "Solo SuperAdmin puede desactivar administradores.";
                    return RedirectToPage(new { Search, Status, Role, page = PageNumber });
                }

                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1)
                {
                    TempData["FlashError"] = "No puedes desactivar al último administrador.";
                    return RedirectToPage(new { Search, Status, Role, page = PageNumber });
                }

                var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
                if (roles.Contains("SuperAdmin") && superAdmins.Count <= 1)
                {
                    TempData["FlashError"] = "No puedes desactivar al último SuperAdmin.";
                    return RedirectToPage(new { Search, Status, Role, page = PageNumber });
                }
            }

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;
            await _userManager.UpdateAsync(user);
            await LogAuditAsync("ToggleActive", user.Id, new { to = "inactive" });
            TempData["FlashSuccess"] = "Usuario desactivado.";
        }
        else
        {
            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);
            await LogAuditAsync("ToggleActive", user.Id, new { to = "active" });
            TempData["FlashSuccess"] = "Usuario activado.";
        }

        return RedirectToPage(new { Search, Status, Role, page = PageNumber });
    }

    public async Task<IActionResult> OnPostMakeAdminAsync(string id)
    {
        if (!User.IsInRole("SuperAdmin"))
        {
            TempData["FlashError"] = "Solo SuperAdmin puede asignar rol Admin.";
            return RedirectToPage(new { Search, Status, Role, page = PageNumber });
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["FlashError"] = "Usuario no encontrado.";
            return RedirectToPage();
        }

        var res = await _userManager.AddToRoleAsync(user, "Admin");
        if (!res.Succeeded)
        {
            TempData["FlashError"] = "No se pudo asignar rol Admin.";
            _logger.LogWarning("MakeAdmin failed: {Errors}", string.Join("; ", res.Errors.Select(e => e.Description)));
        }
        else
        {
            await LogAuditAsync("MakeAdmin", user.Id, new { role = "Admin" });
            TempData["FlashSuccess"] = "Rol Admin asignado.";
        }

        return RedirectToPage(new { Search, Status, Role, page = PageNumber });
    }

    public async Task<IActionResult> OnPostRemoveAdminAsync(string id)
    {
        if (!User.IsInRole("SuperAdmin"))
        {
            TempData["FlashError"] = "Solo SuperAdmin puede revocar rol Admin.";
            return RedirectToPage(new { Search, Status, Role, page = PageNumber });
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["FlashError"] = "Usuario no encontrado.";
            return RedirectToPage();
        }

        var targetRoles = await _userManager.GetRolesAsync(user);
        if (targetRoles.Contains("SuperAdmin"))
        {
            var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
            if (superAdmins.Count <= 1)
            {
                TempData["FlashError"] = "No puedes quitar Admin al último SuperAdmin.";
                return RedirectToPage(new { Search, Status, Role, page = PageNumber });
            }
        }

        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        if (admins.Count <= 1)
        {
            TempData["FlashError"] = "No puedes quitar el rol al último administrador.";
            return RedirectToPage(new { Search, Status, Role, page = PageNumber });
        }

        var res = await _userManager.RemoveFromRoleAsync(user, "Admin");
        if (!res.Succeeded)
        {
            TempData["FlashError"] = "No se pudo quitar rol Admin.";
            _logger.LogWarning("RemoveAdmin failed: {Errors}", string.Join("; ", res.Errors.Select(e => e.Description)));
        }
        else
        {
            await LogAuditAsync("RemoveAdmin", user.Id, new { role = "Admin" });
            TempData["FlashSuccess"] = "Rol Admin removido.";
        }

        return RedirectToPage(new { Search, Status, Role, page = PageNumber });
    }

    public string PageUrl(int page)
    {
        var qs = new Dictionary<string, string?>
        {
            ["page"] = page.ToString(),
            ["Search"] = Search,
            ["Status"] = Status,
            ["Role"] = Role
        };

        var parts = qs.Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");
        return "/Admin/Usuarios" + (parts.Any() ? "?" + string.Join("&", parts) : "");
    }

    private async Task LogAuditAsync(string action, string targetUserId, object details)
    {
        try
        {
            var actorUserId = _userManager.GetUserId(User) ?? string.Empty;
            var payload = JsonSerializer.Serialize(details);

            _context.AdminAuditEvents.Add(new AdminAuditEvent
            {
                ActorUserId = actorUserId,
                TargetUserId = targetUserId,
                Action = action,
                DetailsJson = payload,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo registrar auditoría admin (Action={Action})", action);
        }
    }

    public sealed class Row
    {
        public string UserId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Cedula { get; set; } = "";
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool IsAdmin { get; set; }
    }
}

