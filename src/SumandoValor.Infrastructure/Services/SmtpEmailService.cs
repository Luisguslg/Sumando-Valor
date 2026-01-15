using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SumandoValor.Infrastructure.Services;

public sealed class SmtpEmailService : IEmailService
{
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly SmtpEmailOptions _options;

    public SmtpEmailService(ILogger<SmtpEmailService> logger, IOptions<SmtpEmailOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        if (!_options.Enabled)
        {
            throw new InvalidOperationException("SMTP no está habilitado. Configure Email:Smtp:Enabled=true en producción.");
        }

        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.FromAddress))
        {
            throw new InvalidOperationException("Configuración SMTP incompleta. Verifique Email:Smtp:Host y Email:Smtp:FromAddress.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(to);

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(_options.User))
        {
            client.Credentials = new NetworkCredential(_options.User, _options.Password);
        }
        else
        {
            client.Credentials = CredentialCache.DefaultNetworkCredentials;
        }

        try
        {
            await client.SendMailAsync(message);
            _logger.LogInformation("Email SMTP enviado: To={To}, Subject={Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando email SMTP: To={To}, Subject={Subject}", to, subject);
            throw;
        }
    }

    public Task SendEmailConfirmationAsync(string to, string confirmationLink)
    {
        var body =
            "Por favor confirma tu email haciendo clic en el siguiente enlace:\n\n" +
            $"{confirmationLink}\n\n" +
            "Este enlace expirará en 24 horas.";

        return SendEmailAsync(to, "Confirma tu email - Sumando Valor", body);
    }

    public Task SendPasswordResetAsync(string to, string resetLink)
    {
        var body =
            "Has solicitado restablecer tu contraseña. Haz clic en el siguiente enlace:\n\n" +
            $"{resetLink}\n\n" +
            "Si no solicitaste este cambio, ignora este mensaje.\n\n" +
            "Este enlace expirará en 1 hora.";

        return SendEmailAsync(to, "Restablecer contraseña - Sumando Valor", body);
    }
}

