using Microsoft.Extensions.Logging;

namespace SumandoValor.Infrastructure.Services;

public class DevelopmentEmailService : IEmailService
{
    private readonly ILogger<DevelopmentEmailService> _logger;

    public DevelopmentEmailService(ILogger<DevelopmentEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("=== EMAIL SIMULADO (Development) ===");
        _logger.LogInformation("Para: {To}", to);
        _logger.LogInformation("Asunto: {Subject}", subject);
        _logger.LogInformation("Cuerpo: {Body}", body);
        _logger.LogInformation("===================================");
        return Task.CompletedTask;
    }

    public Task SendEmailConfirmationAsync(string to, string confirmationLink)
    {
        var body = $"Por favor confirma tu email haciendo clic en el siguiente enlace:\n\n{confirmationLink}";
        return SendEmailAsync(to, "Confirma tu email - Sumando Valor", body);
    }
}
