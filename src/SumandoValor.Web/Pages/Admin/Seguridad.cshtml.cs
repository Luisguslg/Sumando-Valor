using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;

namespace SumandoValor.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class SeguridadModel : PageModel
{
    private readonly IdentityOptions _identityOptions;
    private readonly IConfiguration _configuration;

    public SeguridadModel(IOptions<IdentityOptions> identityOptions, IConfiguration configuration)
    {
        _identityOptions = identityOptions?.Value ?? new IdentityOptions();
        _configuration = configuration;
    }

    public PasswordPolicyViewModel PasswordPolicy { get; set; } = new();
    public LockoutPolicyViewModel LockoutPolicy { get; set; } = new();
    public CaptchaPolicyViewModel CaptchaPolicy { get; set; } = new();
    public ConnectionInfoViewModel ConnectionInfo { get; set; } = new();

    public void OnGet()
    {
        var p = _identityOptions.Password;
        PasswordPolicy = new PasswordPolicyViewModel
        {
            RequiredLength = p.RequiredLength,
            RequireDigit = p.RequireDigit,
            RequireLowercase = p.RequireLowercase,
            RequireUppercase = p.RequireUppercase,
            RequireNonAlphanumeric = p.RequireNonAlphanumeric,
            RequiredUniqueChars = p.RequiredUniqueChars
        };

        var l = _identityOptions.Lockout;
        LockoutPolicy = new LockoutPolicyViewModel
        {
            MaxFailedAccessAttempts = l.MaxFailedAccessAttempts,
            DefaultLockoutTimeSpanMinutes = (int)l.DefaultLockoutTimeSpan.TotalMinutes,
            AllowedForNewUsers = l.AllowedForNewUsers
        };

        var captchaProvider = _configuration["Captcha:Provider"] ?? "None";
        var siteKey = _configuration["Captcha:CloudflareTurnstile:SiteKey"] ?? "";
        var secretKey = _configuration["Captcha:CloudflareTurnstile:SecretKey"] ?? "";
        CaptchaPolicy = new CaptchaPolicyViewModel
        {
            Provider = captchaProvider,
            IsEnabled = captchaProvider != "None",
            SiteKeyConfigured = !string.IsNullOrWhiteSpace(siteKey),
            SecretKeyConfigured = !string.IsNullOrWhiteSpace(secretKey),
            UsedOnLogin = true,
            UsedOnRegister = true,
            UsedOnContact = true
        };

        try
        {
            var connStr = _configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrWhiteSpace(connStr))
            {
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connStr);
                ConnectionInfo = new ConnectionInfoViewModel
                {
                    DataSource = builder.DataSource,
                    InitialCatalog = builder.InitialCatalog,
                    IntegratedSecurity = builder.IntegratedSecurity,
                    HasUserId = !string.IsNullOrWhiteSpace(builder.UserID)
                };
            }
        }
        catch
        {
            ConnectionInfo = new ConnectionInfoViewModel { DataSource = "(no parseable)" };
        }
    }

    public class PasswordPolicyViewModel
    {
        public int RequiredLength { get; set; }
        public bool RequireDigit { get; set; }
        public bool RequireLowercase { get; set; }
        public bool RequireUppercase { get; set; }
        public bool RequireNonAlphanumeric { get; set; }
        public int RequiredUniqueChars { get; set; }
    }

    public class LockoutPolicyViewModel
    {
        public int MaxFailedAccessAttempts { get; set; }
        public int DefaultLockoutTimeSpanMinutes { get; set; }
        public bool AllowedForNewUsers { get; set; }
    }

    public class CaptchaPolicyViewModel
    {
        public string Provider { get; set; } = "";
        public bool IsEnabled { get; set; }
        public bool SiteKeyConfigured { get; set; }
        public bool SecretKeyConfigured { get; set; }
        public bool UsedOnLogin { get; set; }
        public bool UsedOnRegister { get; set; }
        public bool UsedOnContact { get; set; }
    }

    public class ConnectionInfoViewModel
    {
        public string DataSource { get; set; } = "";
        public string InitialCatalog { get; set; } = "";
        public bool IntegratedSecurity { get; set; }
        public bool HasUserId { get; set; }
    }
}
