using System.Text.Json;

namespace SumandoValor.Infrastructure.Services;

/// <summary>
/// Dev-only email store that persists messages to disk so they survive restarts.
/// </summary>
public sealed class FileDevEmailStore : IDevEmailStore
{
    private readonly string _dir;

    public FileDevEmailStore(string directoryPath)
    {
        _dir = directoryPath;
        Directory.CreateDirectory(_dir);
    }

    public void AddEmail(string to, string subject, string body)
    {
        Directory.CreateDirectory(_dir);

        var email = new DevEmail
        {
            To = to,
            Subject = subject,
            Body = body,
            CreatedAt = DateTime.UtcNow
        };

        var fileName = $"{email.CreatedAt:yyyyMMdd_HHmmss_fff}_{SanitizeFileName(to)}.json";
        var path = Path.Combine(_dir, fileName);

        var json = JsonSerializer.Serialize(email, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    public List<DevEmail> GetEmails()
    {
        if (!Directory.Exists(_dir))
        {
            return new List<DevEmail>();
        }

        var files = Directory.GetFiles(_dir, "*.json")
            .OrderByDescending(f => f)
            .Take(200)
            .ToList();

        var emails = new List<DevEmail>();
        foreach (var file in files)
        {
            try
            {
                var json = File.ReadAllText(file);
                var email = JsonSerializer.Deserialize<DevEmail>(json);
                if (email != null)
                {
                    emails.Add(email);
                }
            }
            catch
            {
                // ignore corrupted entries
            }
        }

        return emails.OrderByDescending(e => e.CreatedAt).ToList();
    }

    public DevEmail? GetLatestEmail()
    {
        return GetEmails().FirstOrDefault();
    }

    private static string SanitizeFileName(string input)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var safe = new string(input.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
        return safe.Length > 40 ? safe.Substring(0, 40) : safe;
    }
}

