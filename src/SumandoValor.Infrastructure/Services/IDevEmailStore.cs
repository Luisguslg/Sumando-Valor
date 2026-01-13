namespace SumandoValor.Infrastructure.Services;

public interface IDevEmailStore
{
    void AddEmail(string to, string subject, string body);
    List<DevEmail> GetEmails();
    DevEmail? GetLatestEmail();
}

public class DevEmail
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
