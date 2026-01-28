using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SumandoValor.Domain.Helpers;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ProfileModel> _logger;

    public ProfileModel(UserManager<ApplicationUser> userManager, ILogger<ProfileModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public ApplicationUser? CurrentUser { get; set; }

    public async Task OnGetAsync()
    {
        CurrentUser = await _userManager.GetUserAsync(User);
        if (CurrentUser != null)
        {
            Input.Telefono = CurrentUser.Telefono;
            Input.NivelEducativo = CurrentUser.NivelEducativo;
            Input.SituacionLaboral = CurrentUser.SituacionLaboral;
            Input.Sector = CurrentUser.Sector;
            Input.Pais = CurrentUser.Pais;
            Input.Estado = CurrentUser.Estado;
            Input.Municipio = CurrentUser.Municipio;
            Input.TieneDiscapacidad = CurrentUser.TieneDiscapacidad;
            Input.DiscapacidadDescripcion = CurrentUser.DiscapacidadDescripcion;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            return Page();
        }

        if (Input.TieneDiscapacidad && string.IsNullOrWhiteSpace(Input.DiscapacidadDescripcion))
        {
            ModelState.AddModelError(nameof(Input.DiscapacidadDescripcion), "La descripción de la discapacidad es requerida cuando se indica tener una discapacidad.");
            CurrentUser = await _userManager.GetUserAsync(User);
            return Page();
        }

        // Validar municipio pertenece al estado si es Venezuela
        if (Input.Pais == "Venezuela" && !string.IsNullOrWhiteSpace(Input.Estado) && !string.IsNullOrWhiteSpace(Input.Municipio))
        {
            if (Domain.Helpers.Catalogos.MunicipiosPorEstado.TryGetValue(Input.Estado, out var municipiosValidos))
            {
                if (!municipiosValidos.Contains(Input.Municipio))
                {
                    ModelState.AddModelError(nameof(Input.Municipio), "El municipio seleccionado no pertenece al estado seleccionado.");
                    CurrentUser = await _userManager.GetUserAsync(User);
                    return Page();
                }
            }
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        // Sanitización de datos de entrada para seguridad IPCR
        // Nota: Razor Pages escapa HTML automáticamente al renderizar
        user.Telefono = Input.Telefono?.Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "").Trim();
        user.NivelEducativo = Input.NivelEducativo?.Trim() ?? string.Empty;
        user.SituacionLaboral = Input.SituacionLaboral?.Trim() ?? string.Empty;
        user.Sector = Input.Sector?.Trim() ?? string.Empty;
        user.Pais = Input.Pais?.Trim() ?? string.Empty;
        user.Estado = Input.Pais == "Venezuela" ? (Input.Estado?.Trim() ?? null) : null;
        user.Municipio = Input.Pais == "Venezuela" ? (Input.Municipio?.Trim() ?? null) : null;
        user.TieneDiscapacidad = Input.TieneDiscapacidad;
        user.DiscapacidadDescripcion = Input.TieneDiscapacidad ? Input.DiscapacidadDescripcion?.Trim() : null;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            _logger.LogInformation("Usuario actualizó su perfil. UserId={UserId}", user.Id);
            TempData["FlashSuccess"] = "Perfil actualizado exitosamente.";
            return RedirectToPage();
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        CurrentUser = user;
        return Page();
    }

    public class InputModel
    {
        [StringLength(25, ErrorMessage = "El teléfono no puede exceder 25 caracteres")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El nivel educativo es requerido")]
        [Display(Name = "Nivel Educativo")]
        public string NivelEducativo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La situación laboral es requerida")]
        [Display(Name = "Situación Laboral")]
        public string SituacionLaboral { get; set; } = string.Empty;

        [Required(ErrorMessage = "El sector es requerido")]
        [Display(Name = "Sector")]
        public string Sector { get; set; } = string.Empty;

        [Required(ErrorMessage = "El país es requerido")]
        [Display(Name = "País")]
        public string Pais { get; set; } = string.Empty;

        [Display(Name = "Estado")]
        public string? Estado { get; set; }

        [Display(Name = "Municipio")]
        public string? Municipio { get; set; }

        [Display(Name = "Tiene Discapacidad")]
        public bool TieneDiscapacidad { get; set; }

        [StringLength(120, ErrorMessage = "La descripción no puede exceder 120 caracteres")]
        [Display(Name = "Descripción de Discapacidad")]
        public string? DiscapacidadDescripcion { get; set; }
    }
}
