using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;
using SumandoValor.Infrastructure.Services;
using SumandoValor.Web.Services;

namespace SumandoValor.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICaptchaValidator _captchaValidator;
    private readonly IMathCaptchaChallengeService _mathCaptcha;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginModel> _logger;
    private readonly AppDbContext _context;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ICaptchaValidator captchaValidator,
        IMathCaptchaChallengeService mathCaptcha,
        IConfiguration configuration,
        ILogger<LoginModel> logger,
        AppDbContext context)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _captchaValidator = captchaValidator;
        _mathCaptcha = mathCaptcha;
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }
    public bool ShowCaptcha { get; set; }
    public string? CaptchaSiteKey { get; set; }
    public string? CaptchaQuestion { get; set; }

    public Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        var captchaProvider = _configuration["Captcha:Provider"] ?? "Math";
        ShowCaptcha = captchaProvider != "None";
        CaptchaSiteKey = _configuration["Captcha:CloudflareTurnstile:SiteKey"];
        if (captchaProvider == "Math")
            CaptchaQuestion = _mathCaptcha.GetChallenge();
        return Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        var captchaProvider = _configuration["Captcha:Provider"] ?? "Math";
        ShowCaptcha = captchaProvider != "None";
        CaptchaSiteKey = _configuration["Captcha:CloudflareTurnstile:SiteKey"];

        if (ShowCaptcha)
        {
            if (string.IsNullOrWhiteSpace(Input.CaptchaToken))
            {
                ModelState.AddModelError(string.Empty, captchaProvider == "Math"
                    ? "Responde la pregunta de verificación."
                    : "Debes completar la verificación de seguridad (captcha).");
                return Page();
            }
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            var captchaValid = await _captchaValidator.ValidateAsync(Input.CaptchaToken, remoteIp);
            if (!captchaValid)
            {
                ModelState.AddModelError(string.Empty, "La verificación falló. Por favor intenta de nuevo.");
                if (captchaProvider == "Math")
                    CaptchaQuestion = _mathCaptcha.GetChallenge();
                return Page();
            }
        }

        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Debes confirmar tu email antes de iniciar sesión. Por favor revisa tu correo o solicita un nuevo enlace de confirmación.");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario inició sesión exitosamente. UserId={UserId}", user?.Id);
                
                // Restaurar acceso a cursos internos desde cookies después del login
                await RestoreCourseAccessFromCookiesAsync();
                
                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("Intento de login en cuenta bloqueada. UserId={UserId}", user?.Id);
                if (user != null && user.LockoutEnd.HasValue)
                {
                    var lockoutEnd = user.LockoutEnd.Value;
                    if (lockoutEnd > DateTimeOffset.UtcNow)
                    {
                        var remainingTime = lockoutEnd - DateTimeOffset.UtcNow;
                        var minutes = (int)remainingTime.TotalMinutes;
                        ModelState.AddModelError(string.Empty, $"Tu cuenta ha sido bloqueada temporalmente debido a múltiples intentos fallidos. Intenta nuevamente en aproximadamente {minutes} minuto(s).");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Tu cuenta ha sido bloqueada temporalmente. Por favor intenta nuevamente.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Tu cuenta ha sido bloqueada temporalmente debido a múltiples intentos fallidos. Por favor intenta más tarde.");
                }
                return Page();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                return Page();
            }
        }

        return Page();
    }

    private async Task RestoreCourseAccessFromCookiesAsync()
    {
        var cursosInternos = await _context.Cursos
            .Where(c => c.Estado == EstatusCurso.Activo && !c.EsPublico && !string.IsNullOrEmpty(c.TokenAccesoUnico))
            .ToListAsync();

        foreach (var curso in cursosInternos)
        {
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

    public class InputModel
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }

        public string? CaptchaToken { get; set; }
    }
}
