namespace SumandoValor.Infrastructure.Services;

public sealed class SmtpEmailOptions
{
    public bool Enabled { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;

    // If User is empty:
    // - UseDefaultCredentials=true -> uses the process identity (Windows Auth / domain relay scenarios)
    // - UseDefaultCredentials=false -> sends without auth (internal IP-restricted relay scenarios)
    public bool UseDefaultCredentials { get; set; } = false;

    public string? User { get; set; }
    public string? Password { get; set; }

    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Sumando Valor";
}

