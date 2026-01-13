namespace SumandoValor.Infrastructure.Services;

public interface ICaptchaValidator
{
    Task<bool> ValidateAsync(string token);
}
