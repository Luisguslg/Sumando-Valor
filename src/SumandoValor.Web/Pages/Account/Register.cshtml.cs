using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using SumandoValor.Domain.Entities;
using SumandoValor.Domain.Helpers;
using SumandoValor.Infrastructure.Data;
using SumandoValor.Infrastructure.Services;

namespace SumandoValor.Web.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly ICaptchaValidator _captchaValidator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegisterModel> _logger;

    private readonly AppDbContext _context;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailService emailService,
        ICaptchaValidator captchaValidator,
        IConfiguration configuration,
        ILogger<RegisterModel> logger,
        AppDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _captchaValidator = captchaValidator;
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }
    public bool ShowCaptcha { get; set; }
    public string? CaptchaSiteKey { get; set; }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        var captchaProvider = _configuration["Captcha:Provider"] ?? "None";
        ShowCaptcha = captchaProvider != "None";
        CaptchaSiteKey = _configuration["Captcha:CloudflareTurnstile:SiteKey"];
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        var captchaProvider = _configuration["Captcha:Provider"] ?? "None";
        ShowCaptcha = captchaProvider != "None";
        CaptchaSiteKey = _configuration["Captcha:CloudflareTurnstile:SiteKey"];

        if (ShowCaptcha && !string.IsNullOrWhiteSpace(Input.CaptchaToken))
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            var captchaValid = await _captchaValidator.ValidateAsync(Input.CaptchaToken, remoteIp);
            if (!captchaValid)
            {
                ModelState.AddModelError(string.Empty, "La validación del captcha falló. Por favor intenta nuevamente.");
                return Page();
            }
        }

        if (Input.TieneDiscapacidad && string.IsNullOrWhiteSpace(Input.DiscapacidadDescripcion))
        {
            ModelState.AddModelError(nameof(Input.DiscapacidadDescripcion), "La descripción de la discapacidad es requerida cuando se indica tener una discapacidad.");
        }

        if (Input.Pais == "Venezuela")
        {
            if (string.IsNullOrWhiteSpace(Input.Estado))
            {
                ModelState.AddModelError(nameof(Input.Estado), "El estado es requerido cuando el país es Venezuela.");
            }
            if (string.IsNullOrWhiteSpace(Input.Municipio))
            {
                ModelState.AddModelError(nameof(Input.Municipio), "El municipio es requerido cuando el país es Venezuela.");
            }
            else if (!string.IsNullOrWhiteSpace(Input.Estado))
            {
                // Validar que el municipio pertenezca al estado seleccionado
                if (Domain.Helpers.Catalogos.MunicipiosPorEstado.TryGetValue(Input.Estado, out var municipiosValidos))
                {
                    if (!municipiosValidos.Contains(Input.Municipio))
                    {
                        ModelState.AddModelError(nameof(Input.Municipio), "El municipio seleccionado no pertenece al estado seleccionado.");
                    }
                }
            }
        }

        var cedulaValue = Input.Cedula.Trim();
        if (cedulaValue.StartsWith("V-") || cedulaValue.StartsWith("E-"))
        {
            var tipo = cedulaValue.Substring(0, 1);
            var numeros = cedulaValue.Substring(2).Replace("-", "");
            if (tipo == "V" && numeros.Length > 8)
            {
                ModelState.AddModelError(nameof(Input.Cedula), "La cédula venezolana no puede exceder 8 dígitos.");
            }
            Input.Cedula = tipo + "-" + numeros;
        }
        else if (cedulaValue.StartsWith("V") || cedulaValue.StartsWith("E"))
        {
            var tipo = cedulaValue.Substring(0, 1);
            var numeros = cedulaValue.Substring(1).Replace("-", "");
            if (tipo == "V" && numeros.Length > 8)
            {
                ModelState.AddModelError(nameof(Input.Cedula), "La cédula venezolana no puede exceder 8 dígitos.");
            }
            Input.Cedula = tipo + "-" + numeros;
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // If the email already exists, don't just say "ya estás registrado" when the real issue is
        // that the user might be pending confirmation (and SMTP may have failed previously).
        var existing = await _userManager.FindByEmailAsync(Input.Email);
        if (existing != null)
        {
            if (existing.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Este email ya está registrado.");
                return Page();
            }

            try
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(existing);
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId = existing.Id, code = code },
                    protocol: Request.Scheme);

                await _emailService.SendEmailConfirmationAsync(existing.Email!, callbackUrl ?? string.Empty);
                _logger.LogInformation("Reenvío de confirmación de email solicitado. UserId={UserId}", existing.Id);

                TempData["FlashInfo"] = "Este email ya estaba registrado pero aún no estaba confirmado. Te reenviamos el correo de confirmación.";
                return RedirectToPage("./Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo reenviar el correo de confirmación. UserId={UserId}", existing.Id);
                ModelState.AddModelError(string.Empty, "Este email ya está registrado, pero no pudimos reenviar el correo de confirmación. Revisa la configuración de correo en el servidor.");
                return Page();
            }
        }

        // Sanitización de datos de entrada para seguridad IPCR
        // Nota: Razor Pages escapa HTML automáticamente al renderizar, no necesitamos HtmlEncode aquí
        // Solo sanitizamos: Trim, normalización, y validación de formato
        var user = new ApplicationUser
        {
            UserName = Input.Email?.Trim().ToLowerInvariant() ?? string.Empty,
            Email = Input.Email?.Trim().ToLowerInvariant() ?? string.Empty,
            Nombres = Input.Nombres?.Trim() ?? string.Empty,
            Apellidos = Input.Apellidos?.Trim() ?? string.Empty,
            Cedula = Input.Cedula.Trim(),
            Sexo = Input.Sexo?.Trim() ?? string.Empty,
            // UX: keep the form value nullable so the date input is empty on first load.
            // At this point ModelState is valid, so FechaNacimiento has a value.
            FechaNacimiento = Input.FechaNacimiento!.Value,
            TieneDiscapacidad = Input.TieneDiscapacidad,
            DiscapacidadDescripcion = Input.TieneDiscapacidad ? Input.DiscapacidadDescripcion?.Trim() : null,
            NivelEducativo = Input.NivelEducativo?.Trim() ?? string.Empty,
            SituacionLaboral = Input.SituacionLaboral?.Trim() ?? string.Empty,
            Sector = Input.Sector?.Trim() ?? string.Empty,
            CanalConocio = Input.CanalConocio?.Trim() ?? string.Empty,
            Pais = Input.Pais?.Trim() ?? string.Empty,
            Estado = Input.Pais == "Venezuela" ? (Input.Estado?.Trim() ?? null) : null,
            Municipio = Input.Pais == "Venezuela" ? (Input.Municipio?.Trim() ?? null) : null,
            Ciudad = null,
            Telefono = Input.Telefono?.Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "").Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Beneficiario");

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = user.Id, code = code },
                protocol: Request.Scheme);

            try
            {
                await _emailService.SendEmailConfirmationAsync(user.Email!, callbackUrl ?? string.Empty);
                _logger.LogInformation("Nuevo usuario registrado. UserId={UserId}, Email confirmación enviado", user.Id);
                
                // Restaurar acceso a cursos internos desde cookies después del registro
                await RestoreCourseAccessFromCookiesAsync();
                
                TempData["FlashInfo"] = "Registro exitoso. Te enviamos un correo para confirmar tu cuenta antes de iniciar sesión.";
            }
            catch (Exception ex)
            {
                // The user account is created, but they won't be able to sign in until confirmed.
                // We surface a clear message and log the root cause for server troubleshooting.
                _logger.LogError(ex, "Usuario registrado pero falló envío de email de confirmación. UserId={UserId}", user.Id);
                TempData["FlashError"] = "Tu cuenta fue creada, pero no pudimos enviar el correo de confirmación. Contacta al administrador para revisar el correo del servidor.";
            }

            return RedirectToPage("./Login");
        }

        foreach (var error in result.Errors)
        {
            if (error.Code == "DuplicateUserName" || error.Code == "DuplicateEmail")
            {
                ModelState.AddModelError(string.Empty, "Este email ya está registrado.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return Page();
    }

    public class InputModel
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cédula es requerida")]
        [StringLength(20, ErrorMessage = "La cédula no puede exceder 20 caracteres")]
        [Display(Name = "Cédula")]
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La contraseña debe tener al menos 12 caracteres e incluir mayúsculas, minúsculas, números y caracteres especiales.", MinimumLength = 12)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{12,}$", 
            ErrorMessage = "La contraseña debe tener al menos 12 caracteres e incluir mayúsculas, minúsculas, números y caracteres especiales (@$!%*?&).")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son requeridos")]
        [StringLength(80, ErrorMessage = "Los nombres no pueden exceder 80 caracteres")]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [StringLength(80, ErrorMessage = "Los apellidos no pueden exceder 80 caracteres")]
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El sexo es requerido")]
        [Display(Name = "Sexo")]
        public string Sexo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [StringLength(25, ErrorMessage = "El teléfono no puede exceder 25 caracteres")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Display(Name = "Tiene Discapacidad")]
        public bool TieneDiscapacidad { get; set; }

        [StringLength(120, ErrorMessage = "La descripción no puede exceder 120 caracteres")]
        [Display(Name = "Descripción de Discapacidad")]
        public string? DiscapacidadDescripcion { get; set; }

        [Required(ErrorMessage = "El nivel educativo es requerido")]
        [Display(Name = "Nivel Educativo")]
        public string NivelEducativo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La situación laboral es requerida")]
        [Display(Name = "Situación Laboral")]
        public string SituacionLaboral { get; set; } = string.Empty;

        [Required(ErrorMessage = "El canal por el cual conoció es requerido")]
        [Display(Name = "Canal por el cual conoció")]
        public string CanalConocio { get; set; } = string.Empty;

        [Required(ErrorMessage = "El país es requerido")]
        [Display(Name = "País")]
        public string Pais { get; set; } = string.Empty;

        [Display(Name = "Estado")]
        public string? Estado { get; set; }

        [Display(Name = "Municipio")]
        public string? Municipio { get; set; }

        [Required(ErrorMessage = "El sector es requerido")]
        [Display(Name = "Sector")]
        public string Sector { get; set; } = string.Empty;

        [Display(Name = "Token de Captcha")]
        public string? CaptchaToken { get; set; }
    }

    private async Task RestoreCourseAccessFromCookiesAsync()
    {
        // Obtener todos los cursos internos activos
        var cursosInternos = await _context.Cursos
            .Where(c => c.Estado == EstatusCurso.Activo && !c.EsPublico && !string.IsNullOrEmpty(c.TokenAccesoUnico))
            .ToListAsync();

        foreach (var curso in cursosInternos)
        {
            // Verificar si hay una cookie con token válido para este curso
            var cookieToken = Request.Cookies[$"curso_token_{curso.Id}"];
            if (!string.IsNullOrEmpty(cookieToken) &&
                !string.IsNullOrEmpty(curso.TokenAccesoUnico) &&
                curso.TokenAccesoUnico.Equals(cookieToken, StringComparison.Ordinal) &&
                (curso.TokenExpiracion == null || curso.TokenExpiracion > DateTime.UtcNow))
            {
                // Restaurar acceso en sesión desde cookie
                HttpContext.Session.SetString($"curso_access_{curso.Id}", "granted");
            }
        }
    }
}
