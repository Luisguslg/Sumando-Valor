using Microsoft.Extensions.Logging;

namespace SumandoValor.Infrastructure.Services;

public class DevelopmentEmailService : IEmailService
{
    private readonly ILogger<DevelopmentEmailService> _logger;
    private readonly IDevEmailStore _emailStore;

    public DevelopmentEmailService(ILogger<DevelopmentEmailService> logger, IDevEmailStore emailStore)
    {
        _logger = logger;
        _emailStore = emailStore;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("=== EMAIL SIMULADO (Development) ===");
        _logger.LogInformation("Para: {To}", to);
        _logger.LogInformation("Asunto: {Subject}", subject);
        _logger.LogInformation("Cuerpo: {Body}", body);
        _logger.LogInformation("===================================");

        _emailStore.AddEmail(to, subject, body);

        return Task.CompletedTask;
    }

    public Task SendEmailConfirmationAsync(string to, string confirmationLink)
    {
        var body = $"Por favor confirma tu email haciendo clic en el siguiente enlace:\n\n{confirmationLink}\n\nEste enlace expirará en 24 horas.";
        return SendEmailAsync(to, "Confirma tu email - Sumando Valor", body);
    }

    public Task SendPasswordResetAsync(string to, string resetLink)
    {
        var body = $"Has solicitado restablecer tu contraseña. Haz clic en el siguiente enlace:\n\n{resetLink}\n\nSi no solicitaste este cambio, ignora este mensaje.\n\nEste enlace expirará en 1 hora.";
        return SendEmailAsync(to, "Restablecer contraseña - Sumando Valor", body);
    }

    public Task SendCourseAccessLinkAsync(string to, string cursoTitulo, string accessLink)
    {
        var body = $"Has sido invitado a acceder al Programa Formativo: {cursoTitulo}\n\n" +
                   $"Haz clic en el siguiente enlace para acceder:\n\n{accessLink}\n\n" +
                   "Este enlace es único y personal. No lo compartas con otras personas.";
        return SendEmailAsync(to, $"Invitación al Programa Formativo: {cursoTitulo} - Sumando Valor", body);
    }
}
