using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Admin;

[Authorize(Roles = "Moderador,Admin")]
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
    public string? SearchName { get; set; }
    [BindProperty(SupportsGet = true)]
    public string? SearchEmail { get; set; }
    [BindProperty(SupportsGet = true)]
    public string? SearchCedula { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; } // active | inactive

    [BindProperty(SupportsGet = true)]
    public string? Role { get; set; } // Moderador | Beneficiario

    [BindProperty(SupportsGet = true, Name = "page")]
    public int PageNumber { get; set; } = 1;

    [BindProperty]
    public CreateUserInputModel CreateInput { get; set; } = new();

    public List<Row> Rows { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public async Task OnGetAsync()
    {
        var q = BuildQuery();
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
                IsModerador = roles.Contains("Moderador") || roles.Contains("Admin")
            });
        }
    }

    public async Task<IActionResult> OnPostExportCsvAsync()
    {
        var q = BuildQuery();
        var users = await q
            .OrderBy(u => u.Nombres)
            .ThenBy(u => u.Apellidos)
            .ToListAsync();

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Nombres,Apellidos,Email,Cedula,Sexo,Telefono,Estado,Rol");

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            var roleStr = string.Join(";", roles);
            var status = (u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow) ? "Activo" : "Inactivo";
            
            csv.AppendLine($"\"{u.Nombres}\",\"{u.Apellidos}\",\"{u.Email}\",\"{u.Cedula}\",\"{u.Sexo}\",\"{u.Telefono}\",\"{status}\",\"{roleStr}\"");
        }

        var fileName = $"usuarios_{DateTime.Now:yyyyMMdd_HHmm}.csv";
        return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
    }

    private IQueryable<ApplicationUser> BuildQuery()
    {
        var q = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(SearchName))
        {
            var s = SearchName.Trim();
            q = q.Where(u => (u.Nombres + " " + u.Apellidos).Contains(s));
        }
        if (!string.IsNullOrWhiteSpace(SearchEmail))
        {
            q = q.Where(u => u.Email.Contains(SearchEmail.Trim()));
        }
        if (!string.IsNullOrWhiteSpace(SearchCedula))
        {
            q = q.Where(u => u.Cedula.Contains(SearchCedula.Trim()));
        }

        // Legacy generic search support if param is present (optional)
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

        // Role filter (server-side)
        if (!string.IsNullOrWhiteSpace(Role))
        {
            // Note: This needs synchronous execution inside logic or a join.
            // Simplified approach since we need to keep this sync for BuildQuery usually, 
            // but Role filtering involves async usually or joins.
            // Let's use the join approach from original code essentially.
            var roleId = _context.Roles.Where(r => r.Name == Role).Select(r => r.Id).FirstOrDefault();
            if (roleId != null)
            {
                 q = from u in q
                    join ur in _context.UserRoles on u.Id equals ur.UserId
                    where ur.RoleId == roleId
                    select u;
            }
        }
        return q;
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(string id)
    {
        var actorIsAdmin = User.IsInRole("Admin");
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["FlashError"] = "Usuario no encontrado.";
            return RedirectToPage();
        }

        var isActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;

        if (isActive)
        {
            // Desactivar: validaciones solo aplican al desactivar
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Moderador") || roles.Contains("Admin"))
            {
                if (!actorIsAdmin)
                {
                    TempData["FlashError"] = "Solo Admin puede desactivar moderadores.";
                    return RedirectToPage(new { Search, Status, Role, page = PageNumber });
                }

                var moderadores = await _userManager.GetUsersInRoleAsync("Moderador");
                if (moderadores.Count <= 1)
                {
                    TempData["FlashError"] = "No puedes desactivar al último moderador.";
                    return RedirectToPage(new { Search, Status, Role, page = PageNumber });
                }

                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (roles.Contains("Admin") && admins.Count <= 1)
                {
                    TempData["FlashError"] = "No puedes desactivar al último Admin.";
                    return RedirectToPage(new { Search, Status, Role, page = PageNumber });
                }
            }
        }

        try
        {
            if (isActive)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
                TempData["FlashSuccess"] = "Usuario desactivado.";
            }
            else
            {
                user.LockoutEnd = null;
                TempData["FlashSuccess"] = "Usuario activado.";
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Error actualizando usuario {UserId}: {Errors}", user.Id, errors);
                TempData["FlashSuccess"] = null;
                TempData["FlashError"] = $"No se pudo actualizar el usuario: {errors}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al activar/desactivar usuario {UserId}", user.Id);
            TempData["FlashSuccess"] = null;
            TempData["FlashError"] = "Ocurrió un error inesperado al procesar la solicitud.";
        }

        return RedirectToPage(new { Search, Status, Role, page = PageNumber });
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        // Re-apply GET-bound query context so redirects keep filters/paging.
        var search = Request.Query["Search"].ToString();
        var searchName = Request.Query["SearchName"].ToString();
        var searchEmail = Request.Query["SearchEmail"].ToString();
        var searchCedula = Request.Query["SearchCedula"].ToString();
        var status = Request.Query["Status"].ToString();
        var role = Request.Query["Role"].ToString();
        var page = int.TryParse(Request.Query["page"].ToString(), out var p) ? p : 1;

        if (!ModelState.IsValid)
        {
            Search = string.IsNullOrWhiteSpace(search) ? null : search;
            SearchName = string.IsNullOrWhiteSpace(searchName) ? null : searchName;
            SearchEmail = string.IsNullOrWhiteSpace(searchEmail) ? null : searchEmail;
            SearchCedula = string.IsNullOrWhiteSpace(searchCedula) ? null : searchCedula;
            Status = string.IsNullOrWhiteSpace(status) ? null : status;
            Role = string.IsNullOrWhiteSpace(role) ? null : role;
            PageNumber = page;
            await OnGetAsync();
            return Page();
        }

        var existing = await _userManager.FindByEmailAsync(CreateInput.Email);
        if (existing != null)
        {
            TempData["FlashError"] = "Ese email ya existe.";
            return RedirectToPage(new { Search = search, SearchName = searchName, SearchEmail = searchEmail, SearchCedula = searchCedula, Status = status, Role = role, page });
        }

        var user = new ApplicationUser
        {
            UserName = CreateInput.Email,
            Email = CreateInput.Email,
            EmailConfirmed = true, // Admin-created accounts should be usable even if SMTP is blocked.
            EmailVerifiedAt = DateTime.UtcNow,
            Nombres = CreateInput.Nombres,
            Apellidos = CreateInput.Apellidos,
            Cedula = CreateInput.Cedula,
            Sexo = CreateInput.Sexo,
            FechaNacimiento = CreateInput.FechaNacimiento!.Value,
            NivelEducativo = CreateInput.NivelEducativo,
            SituacionLaboral = CreateInput.SituacionLaboral,
            Sector = CreateInput.Sector ?? "TercerSector", // Valor por defecto si no se especifica
            CanalConocio = CreateInput.CanalConocio,
            Pais = CreateInput.Pais ?? "Venezuela", // Valor por defecto si no se especifica
            Estado = CreateInput.Estado,
            Municipio = CreateInput.Municipio,
            Ciudad = CreateInput.Ciudad,
            Telefono = CreateInput.Telefono,
            TieneDiscapacidad = false,
            DiscapacidadDescripcion = null,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, CreateInput.Password);
        if (!result.Succeeded)
        {
            TempData["FlashError"] = "No se pudo crear el usuario.";
            _logger.LogWarning("Create user failed: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
            return RedirectToPage(new { Search = search, SearchName = searchName, SearchEmail = searchEmail, SearchCedula = searchCedula, Status = status, Role = role, page });
        }

        // Default role
        var initialRole = CreateInput.InitialRole == "Moderador" ? "Moderador" : "Beneficiario";
        await _userManager.AddToRoleAsync(user, initialRole);

        TempData["FlashSuccess"] = $"Usuario creado ({initialRole}).";
        return RedirectToPage(new { Search = search, SearchName = searchName, SearchEmail = searchEmail, SearchCedula = searchCedula, Status = status, Role = role, page });
    }

    public async Task<IActionResult> OnPostMakeModeradorAsync(string id)
    {
        if (!User.IsInRole("Admin"))
        {
            TempData["FlashError"] = "Solo Admin puede asignar el rol Moderador.";
            return RedirectToPage(new { Search, Status, Role, page = PageNumber });
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["FlashError"] = "Usuario no encontrado.";
            return RedirectToPage();
        }

        // Admins siempre tienen Moderador; no acción necesaria
        if ((await _userManager.GetRolesAsync(user)).Contains("Admin"))
        {
            TempData["FlashInfo"] = "Los Admins ya son Moderador por defecto.";
            return RedirectToPage(new { Search, Status, Role, page = PageNumber });
        }

        var res = await _userManager.AddToRoleAsync(user, "Moderador");
        if (!res.Succeeded)
        {
            TempData["FlashError"] = "No se pudo asignar rol Moderador.";
            _logger.LogWarning("MakeModerador failed: {Errors}", string.Join("; ", res.Errors.Select(e => e.Description)));
        }
        else
        {
            TempData["FlashSuccess"] = "Rol Moderador asignado.";
        }

        return RedirectToPage(new { Search, Status, Role, page = PageNumber });
    }

    public async Task<IActionResult> OnPostRemoveModeradorAsync(string id)
    {
        if (!User.IsInRole("Admin"))
        {
            TempData["FlashError"] = "Solo Admin puede quitar el rol Moderador.";
            return RedirectToPage(new { Search, Status, Role, page = PageNumber });
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["FlashError"] = "Usuario no encontrado.";
            return RedirectToPage();
        }

        var targetRoles = await _userManager.GetRolesAsync(user);
        if (targetRoles.Contains("Admin"))
        {
            // Rule: Admins always keep Moderador
            TempData["FlashError"] = "No puedes quitar el rol Moderador a un Admin.";
            return RedirectToPage(new { Search, Status, Role, page = PageNumber });
        }

        var moderadores = await _userManager.GetUsersInRoleAsync("Moderador");
        if (moderadores.Count <= 1)
        {
            TempData["FlashError"] = "No puedes quitar el rol al último moderador.";
            return RedirectToPage(new { Search, Status, Role, page = PageNumber });
        }

        var res = await _userManager.RemoveFromRoleAsync(user, "Moderador");
        if (!res.Succeeded)
        {
            TempData["FlashError"] = "No se pudo quitar rol Moderador.";
            _logger.LogWarning("RemoveModerador failed: {Errors}", string.Join("; ", res.Errors.Select(e => e.Description)));
        }
        else
        {
            TempData["FlashSuccess"] = "Rol Moderador removido.";
        }

        return RedirectToPage(new { Search, Status, Role, page = PageNumber });
    }

    public string PageUrl(int page)
    {
        var qs = new Dictionary<string, string?>
        {
            ["page"] = page.ToString(),
            ["Search"] = Search,
            ["SearchName"] = SearchName,
            ["SearchEmail"] = SearchEmail,
            ["SearchCedula"] = SearchCedula,
            ["Status"] = Status,
            ["Role"] = Role
        };

        var parts = qs.Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");
        return "/Admin/Usuarios" + (parts.Any() ? "?" + string.Join("&", parts) : "");
    }


    public sealed class Row
    {
        public string UserId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Cedula { get; set; } = "";
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool IsModerador { get; set; }
    }

    public sealed class CreateUserInputModel
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La contraseña debe tener al menos 14 caracteres.", MinimumLength = 14)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son requeridos")]
        [StringLength(80)]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [StringLength(80)]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cédula es requerida")]
        [StringLength(20)]
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "El sexo es requerido")]
        public string Sexo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El nivel educativo es requerido")]
        public string NivelEducativo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La situación laboral es requerida")]
        public string SituacionLaboral { get; set; } = string.Empty;

        [Required(ErrorMessage = "El sector es requerido")]
        public string Sector { get; set; } = string.Empty;

        [Required(ErrorMessage = "El canal por el cual conoció es requerido")]
        public string CanalConocio { get; set; } = string.Empty;

        [Required(ErrorMessage = "El país es requerido")]
        public string Pais { get; set; } = "Venezuela";

        [Required(ErrorMessage = "El estado es requerido")]
        public string Estado { get; set; } = string.Empty;

        public string? Municipio { get; set; }

        [Required(ErrorMessage = "La ciudad es requerida")]
        public string Ciudad { get; set; } = string.Empty;

        [StringLength(25)]
        public string? Telefono { get; set; }

        [Required]
        public string InitialRole { get; set; } = "Beneficiario"; // Beneficiario | Moderador
    }
}

