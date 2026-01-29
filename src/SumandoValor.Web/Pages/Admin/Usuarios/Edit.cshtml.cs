using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Admin.Usuarios;

[Authorize(Roles = "Moderador,Admin")]
public class EditModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<EditModel> _logger;

    public EditModel(AppDbContext context, UserManager<ApplicationUser> userManager, ILogger<EditModel> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool UserNotFound { get; set; }
    public string Email { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Id);
        if (user == null)
        {
            UserNotFound = true;
            return;
        }

        Email = user.Email ?? user.UserName ?? string.Empty;
        Input = new InputModel
        {
            Cedula = user.Cedula ?? string.Empty,
            Nombres = user.Nombres ?? string.Empty,
            Apellidos = user.Apellidos ?? string.Empty,
            Sexo = user.Sexo ?? string.Empty,
            Telefono = user.Telefono,
            IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _userManager.FindByIdAsync(Id);
        if (user == null)
        {
            UserNotFound = true;
            return Page();
        }

        // Only Admin can modify Admins
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Admin") && !User.IsInRole("Admin"))
        {
            TempData["FlashError"] = "Solo Admin puede editar un Admin.";
            return RedirectToPage("/Admin/Usuarios");
        }

        user.Cedula = (Input.Cedula ?? string.Empty).Trim();
        user.Nombres = (Input.Nombres ?? string.Empty).Trim();
        user.Apellidos = (Input.Apellidos ?? string.Empty).Trim();
        user.Sexo = (Input.Sexo ?? string.Empty).Trim();
        user.Telefono = string.IsNullOrWhiteSpace(Input.Telefono) ? null : Input.Telefono.Trim();

        var wantActive = Input.IsActive;
        var isActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;
        if (wantActive != isActive)
        {
            if (!wantActive)
            {
                // Prevent deactivating the last Admin/Moderador unless Admin (same rule as list page)
                var actorIsAdmin = User.IsInRole("Admin");
                if ((await _userManager.GetRolesAsync(user)).Any(r => r is "Admin" or "Moderador") && !actorIsAdmin)
                {
                    ModelState.AddModelError(string.Empty, "Solo Admin puede desactivar moderadores.");
                    Email = user.Email ?? user.UserName ?? "";
                    return Page();
                }

                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }
            else
            {
                user.LockoutEnd = null;
            }
        }

        try
        {
            var res = await _userManager.UpdateAsync(user);
            if (!res.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "No se pudo guardar el usuario.");
                _logger.LogWarning("User update failed: {Errors}", string.Join("; ", res.Errors.Select(e => e.Description)));
                Email = user.Email ?? user.UserName ?? "";
                return Page();
            }

            TempData["FlashSuccess"] = "Usuario actualizado.";
            return RedirectToPage("/Admin/Usuarios");
        }
        catch (DbUpdateException ex)
        {
            // Likely unique constraint (Cedula/Email)
            _logger.LogWarning(ex, "DbUpdateException updating user {UserId}", user.Id);
            ModelState.AddModelError(string.Empty, "No se pudo guardar. Verifica que la cédula no esté repetida.");
            Email = user.Email ?? user.UserName ?? "";
            return Page();
        }
    }

    public sealed class InputModel
    {
        [Required(ErrorMessage = "La cédula es requerida")]
        [StringLength(20, ErrorMessage = "La cédula no puede exceder 20 caracteres")]
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son requeridos")]
        [StringLength(80, ErrorMessage = "Los nombres no pueden exceder 80 caracteres")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [StringLength(80, ErrorMessage = "Los apellidos no pueden exceder 80 caracteres")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El sexo es requerido")]
        public string Sexo { get; set; } = string.Empty;

        [StringLength(25, ErrorMessage = "El teléfono no puede exceder 25 caracteres")]
        public string? Telefono { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

