namespace SumandoValor.Infrastructure.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendEmailConfirmationAsync(string to, string confirmationLink);
    Task SendPasswordResetAsync(string to, string resetLink);
    Task SendCourseAccessLinkAsync(string to, string cursoTitulo, string accessLink);
}
