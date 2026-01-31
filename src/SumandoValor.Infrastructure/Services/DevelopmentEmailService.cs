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

    public Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        _logger.LogInformation("=== EMAIL SIMULADO (Development) ===");
        _logger.LogInformation("Para: {To}", to);
        _logger.LogInformation("Asunto: {Subject}", subject);
        _logger.LogInformation("Cuerpo: {Body}", body.Length > 200 ? body[..200] + "..." : body);
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
        var body = EmailTemplates.CourseAccessLinkHtml(cursoTitulo, accessLink);
        return SendEmailAsync(to, $"Invitación al Programa Formativo: {cursoTitulo} - Sumando Valor", body, isHtml: true);
    }

    public async Task SendCourseAccessLinkToMultipleAsync(IEnumerable<string> emails, string cursoTitulo, string accessLink)
    {
        var list = emails.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim()).Distinct().ToList();
        foreach (var email in list)
        {
            await SendCourseAccessLinkAsync(email, cursoTitulo, accessLink);
        }
    }
}
