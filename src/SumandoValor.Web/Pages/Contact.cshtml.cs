using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;
using SumandoValor.Infrastructure.Services;

namespace SumandoValor.Web.Pages;

public class ContactModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ICaptchaValidator _captchaValidator;
    private readonly ILogger<ContactModel> _logger;

    public ContactModel(
        AppDbContext context,
        IEmailService emailService,
        IConfiguration configuration,
        ICaptchaValidator captchaValidator,
        ILogger<ContactModel> logger)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
        _captchaValidator = captchaValidator;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool ShowCaptcha { get; set; }
    public string? CaptchaSiteKey { get; set; }

    public void OnGet()
    {
        var captchaProvider = _configuration["Captcha:Provider"] ?? "None";
        ShowCaptcha = captchaProvider != "None";
        CaptchaSiteKey = _configuration["Captcha:CloudflareTurnstile:SiteKey"];
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var captchaProvider = _configuration["Captcha:Provider"] ?? "None";
        ShowCaptcha = captchaProvider != "None";
        CaptchaSiteKey = _configuration["Captcha:CloudflareTurnstile:SiteKey"];

        if (ShowCaptcha && !string.IsNullOrWhiteSpace(Input.CaptchaToken))
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            var captchaValid = await _captchaValidator.ValidateAsync(Input.CaptchaToken, remoteIp);
            if (!captchaValid)
            {
                ModelState.AddModelError(string.Empty, "La validación del captcha falló. Por favor intenta nuevamente.");
            }
        }
        else if (ShowCaptcha)
        {
            ModelState.AddModelError(string.Empty, "Debes completar la verificación de seguridad (captcha).");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var msg = new MensajeContacto
        {
            Nombre = Input.Nombre.Trim(),
            Email = Input.Email.Trim(),
            Titulo = Input.Titulo.Trim(),
            Mensaje = Input.Mensaje.Trim(),
            Estado = EstadoMensaje.Nuevo,
            CreatedAt = DateTime.UtcNow
        };

        _context.MensajesContacto.Add(msg);
        await _context.SaveChangesAsync();

        var recipient = _configuration["Email:ContactRecipient"] ?? "fundacionkpmg@kpmg.com";
        var subject = $"[Contacto] {msg.Titulo}";
        var body =
            "Nuevo mensaje de contacto (Sumando Valor)\n\n" +
            $"Nombre: {msg.Nombre}\n" +
            $"Email: {msg.Email}\n" +
            $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}\n\n" +
            "Mensaje:\n" +
            msg.Mensaje + "\n";

        try
        {
            await _emailService.SendEmailAsync(recipient, subject, body);
            TempData["FlashSuccess"] = "Mensaje enviado correctamente. Nos pondremos en contacto contigo pronto.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando correo de contacto. MsgId={MsgId}, Recipient={Recipient}", msg.Id, recipient);
            TempData["FlashError"] =
                "Recibimos tu mensaje, pero ocurrió un problema al enviar el correo. " +
                "Por favor intenta nuevamente o contáctanos directamente.";
            return RedirectToPage();
        }
    }

    public class InputModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El mensaje es requerido")]
        [StringLength(2000)]
        public string Mensaje { get; set; } = string.Empty;

        [Display(Name = "Token de Captcha")]
        public string? CaptchaToken { get; set; }
    }
}
