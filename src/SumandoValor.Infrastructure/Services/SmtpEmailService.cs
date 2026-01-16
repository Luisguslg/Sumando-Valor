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

        // Emit a safe summary to help diagnose relay/auth issues (no secrets).
        _logger.LogInformation(
            "SMTP config: Host={Host}, Port={Port}, EnableSsl={EnableSsl}, UseDefaultCredentials={UseDefaultCredentials}, HasUser={HasUser}, From={From}",
            _options.Host, _options.Port, _options.EnableSsl, _options.UseDefaultCredentials, !string.IsNullOrWhiteSpace(_options.User), _options.FromAddress);

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

        // Best-effort: SmtpClient.Timeout is not reliably honored by SendMailAsync in all environments,
        // so we also enforce a hard timeout below with Task.WhenAny.
        if (_options.TimeoutMs > 0)
            client.Timeout = _options.TimeoutMs;

        if (!string.IsNullOrWhiteSpace(_options.User))
        {
            client.Credentials = new NetworkCredential(_options.User, _options.Password);
        }
        else
        {
            // Three scenarios:
            // 1) Internal relay without auth (recommended if IT restricts by IP): User empty + UseDefaultCredentials=false
            // 2) Relay using Windows Auth (domain): User empty + UseDefaultCredentials=true
            // 3) Authenticated SMTP: User set
            if (_options.UseDefaultCredentials)
            {
                client.UseDefaultCredentials = true;
            }
            else
            {
                client.UseDefaultCredentials = false;
                client.Credentials = null;
            }
        }

        try
        {
            _logger.LogInformation("Enviando email SMTP: To={To}, Subject={Subject}", to, subject);

            var sendTask = client.SendMailAsync(message);
            if (_options.TimeoutMs > 0)
            {
                var completed = await Task.WhenAny(sendTask, Task.Delay(_options.TimeoutMs));
                if (completed != sendTask)
                {
                    throw new TimeoutException($"Timeout enviando email SMTP después de {_options.TimeoutMs}ms (Host={_options.Host}, Port={_options.Port}).");
                }
            }

            await sendTask;
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

