namespace SumandoValor.Infrastructure.Services;

public class MockCaptchaValidator : ICaptchaValidator
{
    public Task<bool> ValidateAsync(string token)
    {
        return Task.FromResult(true);
    }
}
