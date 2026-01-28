using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Infrastructure.Data;
using SumandoValor.Infrastructure.Services;

namespace SumandoValor.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class RolesModel : PageModel
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<RolesModel> _logger;

    public RolesModel(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        AppDbContext context,
        IAuditService auditService,
        ILogger<RolesModel> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    public List<RoleViewModel> Roles { get; set; } = new();
    public List<UserRoleViewModel> Users { get; set; } = new();

    [BindProperty]
    public string? NewRoleName { get; set; }

    public async Task OnGetAsync()
    {
        // Obtener todos los roles
        var allRoles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
        Roles = allRoles.Select(r => new RoleViewModel
        {
            Id = r.Id,
            Name = r.Name ?? "",
            NormalizedName = r.NormalizedName ?? ""
        }).ToList();

        // Obtener todos los usuarios con sus roles
        var allUsers = await _userManager.Users
            .OrderBy(u => u.Nombres)
            .ThenBy(u => u.Apellidos)
            .ToListAsync();

        Users = new List<UserRoleViewModel>();
        foreach (var user in allUsers)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            Users.Add(new UserRoleViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? "",
                FullName = $"{user.Nombres} {user.Apellidos}".Trim(),
                Roles = userRoles.ToList()
            });
        }
    }

    public async Task<IActionResult> OnPostCreateRoleAsync()
    {
        if (string.IsNullOrWhiteSpace(NewRoleName))
        {
            TempData["FlashError"] = "El nombre del rol es requerido.";
            await OnGetAsync();
            return Page();
        }

        var roleName = NewRoleName.Trim();
        if (await _roleManager.RoleExistsAsync(roleName))
        {
            TempData["FlashError"] = $"El rol '{roleName}' ya existe.";
            await OnGetAsync();
            return Page();
        }

        var role = new IdentityRole(roleName);
        var result = await _roleManager.CreateAsync(role);

        if (result.Succeeded)
        {
            var actorUserId = _userManager.GetUserId(User) ?? "";
            var actorEmail = (await _userManager.GetUserAsync(User))?.Email;
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            
            await _auditService.LogChangeAsync(actorUserId, actorEmail, "Rol", role.Id, "Create", null, new { Name = roleName }, ipAddress);
            
            TempData["FlashSuccess"] = $"Rol '{roleName}' creado exitosamente.";
            _logger.LogInformation("Rol {RoleName} creado por {UserId}", roleName, actorUserId);
        }
        else
        {
            TempData["FlashError"] = $"Error al crear el rol: {string.Join(", ", result.Errors.Select(e => e.Description))}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteRoleAsync(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            TempData["FlashError"] = "Rol no encontrado.";
            return RedirectToPage();
        }

        // No permitir eliminar roles del sistema
        var systemRoles = new[] { "Admin", "Moderador", "Beneficiario" };
        if (systemRoles.Contains(role.Name))
        {
            TempData["FlashError"] = "No se pueden eliminar roles del sistema.";
            return RedirectToPage();
        }

        // Verificar si hay usuarios con este rol
        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
        if (usersInRole.Count > 0)
        {
            TempData["FlashError"] = $"No se puede eliminar el rol '{role.Name}' porque tiene {usersInRole.Count} usuario(s) asignado(s).";
            return RedirectToPage();
        }

        var roleName = role.Name ?? "";
        var result = await _roleManager.DeleteAsync(role);

        if (result.Succeeded)
        {
            var actorUserId = _userManager.GetUserId(User) ?? "";
            var actorEmail = (await _userManager.GetUserAsync(User))?.Email;
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            
            await _auditService.LogChangeAsync(actorUserId, actorEmail, "Rol", roleId, "Delete", new { Name = roleName }, null, ipAddress);
            
            TempData["FlashSuccess"] = $"Rol '{roleName}' eliminado exitosamente.";
            _logger.LogInformation("Rol {RoleName} eliminado por {UserId}", roleName, actorUserId);
        }
        else
        {
            TempData["FlashError"] = $"Error al eliminar el rol: {string.Join(", ", result.Errors.Select(e => e.Description))}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateUserRolesAsync(string userId, List<string> selectedRoles)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["FlashError"] = "Usuario no encontrado.";
            return RedirectToPage();
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToAdd = selectedRoles.Except(currentRoles).ToList();
        var rolesToRemove = currentRoles.Except(selectedRoles).ToList();

        // No permitir quitar Admin a un Admin (solo puede hacerlo otro Admin)
        if (currentRoles.Contains("Admin") && !selectedRoles.Contains("Admin") && !User.IsInRole("Admin"))
        {
            TempData["FlashError"] = "Solo Admin puede quitar el rol Admin a otros usuarios.";
            return RedirectToPage();
        }

        // No permitir agregar Admin si no eres Admin
        if (rolesToAdd.Contains("Admin") && !User.IsInRole("Admin"))
        {
            TempData["FlashError"] = "Solo Admin puede asignar el rol Admin.";
            return RedirectToPage();
        }

        var actorUserId = _userManager.GetUserId(User) ?? "";
        var actorEmail = (await _userManager.GetUserAsync(User))?.Email;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        foreach (var role in rolesToAdd)
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            if (result.Succeeded)
            {
                await _auditService.LogUserActionAsync(actorUserId, actorEmail, userId, "AddRole", new { Role = role }, ipAddress);
            }
        }

        foreach (var role in rolesToRemove)
        {
            // No permitir quitar Admin si es el último Admin
            if (role == "Admin")
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1)
                {
                    TempData["FlashError"] = "No se puede quitar el rol Admin al último administrador.";
                    return RedirectToPage();
                }
            }

            var result = await _userManager.RemoveFromRoleAsync(user, role);
            if (result.Succeeded)
            {
                await _auditService.LogUserActionAsync(actorUserId, actorEmail, userId, "RemoveRole", new { Role = role }, ipAddress);
            }
        }

        TempData["FlashSuccess"] = "Roles actualizados exitosamente.";
        return RedirectToPage();
    }

    public class RoleViewModel
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string NormalizedName { get; set; } = "";
    }

    public class UserRoleViewModel
    {
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public List<string> Roles { get; set; } = new();
    }
}
