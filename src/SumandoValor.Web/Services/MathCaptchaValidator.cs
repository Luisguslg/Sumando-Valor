using SumandoValor.Infrastructure.Services;

namespace SumandoValor.Web.Services;

public class MathCaptchaValidator : ICaptchaValidator
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MathCaptchaValidator(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<bool> ValidateAsync(string token, string? remoteIp = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult(false);

        var context = _httpContextAccessor.HttpContext;
        var session = context?.Session;
        if (session == null)
            return Task.FromResult(false);

        var expected = session.GetString(MathCaptchaChallengeService.SessionKey);
        session.Remove(MathCaptchaChallengeService.SessionKey);

        var userAnswer = token.Trim();
        var ok = expected != null && expected == userAnswer;
        return Task.FromResult(ok);
    }
}
