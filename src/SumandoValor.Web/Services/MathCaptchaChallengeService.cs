using System.Security.Cryptography;

namespace SumandoValor.Web.Services;

public class MathCaptchaChallengeService : IMathCaptchaChallengeService
{
    public const string SessionKey = "MathCaptcha_Answer";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MathCaptchaChallengeService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetChallenge()
    {
        var a = RandomNumberGenerator.GetInt32(1, 11);
        var b = RandomNumberGenerator.GetInt32(1, 11);
        var answer = (a + b).ToString();
        var context = _httpContextAccessor.HttpContext;
        if (context?.Session != null)
            context.Session.SetString(SessionKey, answer);
        return $"¿Cuánto es {a} + {b}?";
    }
}
