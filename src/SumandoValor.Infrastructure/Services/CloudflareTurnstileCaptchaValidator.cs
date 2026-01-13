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

    public async Task<bool> ValidateAsync(string token, string? remoteIp = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_secretKey) || string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var formData = new List<KeyValuePair<string, string>>
            {
                new("secret", _secretKey),
                new("response", token)
            };

            if (!string.IsNullOrWhiteSpace(remoteIp))
            {
                formData.Add(new KeyValuePair<string, string>("remoteip", remoteIp));
            }

            var content = new FormUrlEncodedContent(formData);

            var response = await _httpClient.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            return response.IsSuccessStatusCode && responseBody.Contains("\"success\":true");
        }
        catch
        {
            return false;
        }
    }
}
