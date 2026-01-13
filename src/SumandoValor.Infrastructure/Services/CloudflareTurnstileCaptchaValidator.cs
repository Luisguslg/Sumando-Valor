using Microsoft.Extensions.Configuration;

namespace SumandoValor.Infrastructure.Services;

public class CloudflareTurnstileCaptchaValidator : ICaptchaValidator
{
    private readonly HttpClient _httpClient;
    private readonly string _secretKey;
    private readonly string _siteKey;

    public CloudflareTurnstileCaptchaValidator(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _secretKey = configuration["Captcha:CloudflareTurnstile:SecretKey"] ?? string.Empty;
        _siteKey = configuration["Captcha:CloudflareTurnstile:SiteKey"] ?? string.Empty;
    }

    public async Task<bool> ValidateAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(_secretKey) || string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", _secretKey),
                new KeyValuePair<string, string>("response", token)
            });

            var response = await _httpClient.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode && responseBody.Contains("\"success\":true");
        }
        catch
        {
            return false;
        }
    }
}
