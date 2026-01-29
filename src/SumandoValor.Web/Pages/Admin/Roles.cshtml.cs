using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Helpers;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class RolesModel : PageModel
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<RolesModel> _logger;

    public RolesModel(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        AppDbContext context,
        ILogger<RolesModel> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    public List<RoleViewModel> Roles { get; set; } = new();
    public List<UserRoleViewModel> Users { get; set; } = new();
    public Dictionary<string, List<string>> AllPermissions { get; set; } = new();
    public Dictionary<string, List<string>> RolePermissions { get; set; } = new();

    [BindProperty]
    public string? NewRoleName { get; set; }

    [BindProperty]
    public string? SelectedRoleId { get; set; }

    [BindProperty]
    public List<string> SelectedPermissions { get; set; } = new();

    public async Task OnGetAsync(string? roleId = null)
    {
        AllPermissions = Permissions.GetAllPermissions();

        var allRoles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
        Roles = allRoles
            .GroupBy(r => r.Name)
            .Select(g => g.First())
            .Select(r => new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name ?? "",
                NormalizedName = r.NormalizedName ?? ""
            })
            .ToList();

        // Obtener permisos de cada rol
        RolePermissions = new Dictionary<string, List<string>>();
        foreach (var role in Roles)
        {
            var roleEntity = await _roleManager.FindByIdAsync(role.Id);
            if (roleEntity != null)
            {
                var claims = await _roleManager.GetClaimsAsync(roleEntity);
                var permissions = claims
                    .Where(c => c.Type == Permissions.ClaimType)
                    .Select(c => c.Value)
                    .ToList();
                RolePermissions[role.Id] = permissions;
            }
        }

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

        SelectedRoleId = roleId;
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
            // Asignar permisos por defecto según el nombre del rol
            var defaultPermissions = Permissions.GetDefaultPermissionsForRole(roleName);
            foreach (var permission in defaultPermissions)
            {
                var claim = new System.Security.Claims.Claim(Permissions.ClaimType, permission);
                await _roleManager.AddClaimAsync(role, claim);
            }

            var actorUserId = _userManager.GetUserId(User) ?? "";
            
            TempData["FlashSuccess"] = $"Rol '{roleName}' creado exitosamente con permisos por defecto.";
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

        // Solo SuperAdmin (rol Admin) puede asignar/quitar el rol Admin
        if ((rolesToAdd.Contains("Admin") || rolesToRemove.Contains("Admin")) && !User.IsInRole("Admin"))
        {
            TempData["FlashError"] = "Solo SuperAdmin puede gestionar el rol Admin.";
            return RedirectToPage();
        }

        foreach (var role in rolesToAdd)
        {
            await _userManager.AddToRoleAsync(user, role);
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

            await _userManager.RemoveFromRoleAsync(user, role);
        }

        TempData["FlashSuccess"] = "Roles actualizados exitosamente.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateRolePermissionsAsync(string roleId, List<string> selectedPermissions)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            TempData["FlashError"] = "Rol no encontrado.";
            return RedirectToPage();
        }

        // No permitir modificar permisos de roles del sistema (excepto Admin puede modificar Moderador)
        var systemRoles = new[] { "Admin", "Beneficiario" };
        if (systemRoles.Contains(role.Name) && !User.IsInRole("Admin"))
        {
            TempData["FlashError"] = "No se pueden modificar permisos de roles del sistema.";
            return RedirectToPage();
        }

        // Obtener permisos actuales del rol
        var currentClaims = await _roleManager.GetClaimsAsync(role);
        var currentPermissions = currentClaims
            .Where(c => c.Type == Permissions.ClaimType)
            .Select(c => c.Value)
            .ToList();

        // Eliminar permisos que ya no están seleccionados
        var permissionsToRemove = currentPermissions.Except(selectedPermissions ?? new List<string>()).ToList();
        foreach (var permission in permissionsToRemove)
        {
            var claim = currentClaims.FirstOrDefault(c => c.Type == Permissions.ClaimType && c.Value == permission);
            if (claim != null)
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }
        }

        // Agregar nuevos permisos
        var permissionsToAdd = (selectedPermissions ?? new List<string>()).Except(currentPermissions).ToList();
        foreach (var permission in permissionsToAdd)
        {
            var claim = new System.Security.Claims.Claim(Permissions.ClaimType, permission);
            await _roleManager.AddClaimAsync(role, claim);
        }

        var actorUserId = _userManager.GetUserId(User) ?? "";

        TempData["FlashSuccess"] = $"Permisos del rol '{role.Name}' actualizados exitosamente.";
        _logger.LogInformation("Permisos del rol {RoleName} actualizados por {UserId}", role.Name, actorUserId);

        return RedirectToPage(new { roleId });
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
