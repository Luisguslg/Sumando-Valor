using System.Collections.Concurrent;

namespace SumandoValor.Infrastructure.Services;

public class InMemoryDevEmailStore : IDevEmailStore
{
    private readonly ConcurrentBag<DevEmail> _emails = new();

    public void AddEmail(string to, string subject, string body)
    {
        _emails.Add(new DevEmail
        {
            To = to,
            Subject = subject,
            Body = body,
            CreatedAt = DateTime.UtcNow
        });
    }

    public List<DevEmail> GetEmails()
    {
        return _emails.OrderByDescending(e => e.CreatedAt).ToList();
    }

    public DevEmail? GetLatestEmail()
    {
        return _emails.OrderByDescending(e => e.CreatedAt).FirstOrDefault();
    }
}
