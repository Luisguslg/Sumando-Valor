using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailService emailService,
        ICaptchaValidator captchaValidator,
        IConfiguration configuration,
        ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _captchaValidator = captchaValidator;
        _configuration = configuration;
        _logger = logger;
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

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            Nombres = Input.Nombres,
            Apellidos = Input.Apellidos,
            Cedula = Input.Cedula,
            Sexo = Input.Sexo,
            FechaNacimiento = Input.FechaNacimiento,
            TieneDiscapacidad = Input.TieneDiscapacidad,
            DiscapacidadDescripcion = Input.TieneDiscapacidad ? Input.DiscapacidadDescripcion : null,
            NivelEducativo = Input.NivelEducativo,
            SituacionLaboral = Input.SituacionLaboral,
            CanalConocio = Input.CanalConocio,
            Estado = Input.Estado,
            Ciudad = Input.Ciudad,
            Telefono = Input.Telefono,
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

            await _emailService.SendEmailConfirmationAsync(user.Email, callbackUrl ?? string.Empty);

            _logger.LogInformation("Usuario {Email} se registró. Email de confirmación enviado.", user.Email);

            TempData["Message"] = "Registro exitoso. Por favor revisa tu email para confirmar tu cuenta antes de iniciar sesión. En modo Development, puedes ver el enlace en /Dev/Emails";
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
        [RegularExpression(@"^[VE]?\d{6,9}$", ErrorMessage = "Formato de cédula inválido")]
        [Display(Name = "Cédula")]
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La contraseña debe tener al menos {2} caracteres.", MinimumLength = 8)]
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
        public DateTime FechaNacimiento { get; set; }

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

        [Required(ErrorMessage = "El estado es requerido")]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad es requerida")]
        [Display(Name = "Ciudad")]
        public string Ciudad { get; set; } = string.Empty;

        [Display(Name = "Token de Captcha")]
        public string? CaptchaToken { get; set; }
    }
}
