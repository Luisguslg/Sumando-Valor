namespace SumandoValor.Infrastructure.Services;

public interface ICaptchaValidator
{
    Task<bool> ValidateAsync(string token, string? remoteIp = null, CancellationToken cancellationToken = default);
}
