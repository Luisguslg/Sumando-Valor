using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SumandoValor.Infrastructure.Services;

namespace SumandoValor.Web.Pages.Admin;

[Authorize(Roles = "Moderador,Admin")]
public class EmailDiagnosticsModel : PageModel
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailDiagnosticsModel> _logger;

    public EmailDiagnosticsModel(IEmailService emailService, ILogger<EmailDiagnosticsModel> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool LastResultOk { get; set; }
    public string? LastResult { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var subject = $"[Sumando Valor] Prueba de correo ({DateTime.Now:yyyy-MM-dd HH:mm})";
            var body =
                "Este es un correo de prueba enviado desde el módulo Admin.\n\n" +
                "Si lo recibiste, la configuración de correo está funcionando.\n";

            await _emailService.SendEmailAsync(Input.To, subject, body);
            LastResultOk = true;
            LastResult = "Enviado correctamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo en EmailDiagnostics hacia {To}", Input.To);
            LastResultOk = false;
            LastResult = $"Error: {ex.Message}";
        }

        return Page();
    }

    public sealed class InputModel
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Enviar a")]
        public string To { get; set; } = string.Empty;
    }
}

