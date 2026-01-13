namespace SumandoValor.Infrastructure.Services;

public class MockCaptchaValidator : ICaptchaValidator
{
    public Task<bool> ValidateAsync(string token, string? remoteIp = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}
