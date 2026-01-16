using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Infrastructure.Data;
using SumandoValor.Infrastructure.Services;
using SumandoValor.Web.Services.Certificates;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.SignIn.RequireConfirmedEmail = true;
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.SlidingExpiration = true;
});

builder.Services.AddHttpClient();

// Dev email storage: persist to disk so emails survive restarts (used by /Dev/Emails).
var devEmailPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DevEmails");
builder.Services.AddSingleton<IDevEmailStore>(_ => new FileDevEmailStore(devEmailPath));

builder.Services.Configure<SmtpEmailOptions>(builder.Configuration.GetSection("Email:Smtp"));
// Compatibility: support legacy keys from the old ASP.NET MVC app (Ejemplo/Web.config).
// This allows configuring SMTP via IIS environment variables like ServidorCorreo/PuertoCorreo/etc.
builder.Services.PostConfigure<SmtpEmailOptions>(options =>
{
    var cfg = builder.Configuration;

    if (bool.TryParse(cfg["EnviarCorreo"], out var legacyEnabled))
        options.Enabled = legacyEnabled;

    if (string.IsNullOrWhiteSpace(options.Host))
        options.Host = cfg["ServidorCorreo"] ?? options.Host;

    if (int.TryParse(cfg["PuertoCorreo"], out var legacyPort))
        options.Port = legacyPort;

    if (bool.TryParse(cfg["Enablessl"], out var legacySsl))
        options.EnableSsl = legacySsl;

    var legacyFrom = cfg["CorreoDeServicios"];
    if (!string.IsNullOrWhiteSpace(legacyFrom))
    {
        // Prefer configured FromAddress, but fall back to legacy "CorreoDeServicios"
        if (string.IsNullOrWhiteSpace(options.FromAddress))
            options.FromAddress = legacyFrom;

        // If password is present and user not set, use the same account as user
        if (string.IsNullOrWhiteSpace(options.User) && !string.IsNullOrWhiteSpace(cfg["CorreoPassword"]))
            options.User = legacyFrom;
    }

    if (string.IsNullOrWhiteSpace(options.Password))
        options.Password = cfg["CorreoPassword"] ?? options.Password;
});

var captchaProvider = builder.Configuration["Captcha:Provider"] ?? "None";
if (builder.Environment.IsDevelopment() || captchaProvider == "None")
{
    builder.Services.AddScoped<ICaptchaValidator, MockCaptchaValidator>();
}
else if (captchaProvider == "Turnstile")
{
    builder.Services.AddScoped<ICaptchaValidator, CloudflareTurnstileCaptchaValidator>();
}

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IEmailService, DevelopmentEmailService>();
}
else
{
    builder.Services.AddScoped<IEmailService, SmtpEmailService>();
}

builder.Services.AddRazorPages();
builder.Services.AddSingleton<CertificatePdfGenerator>();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        await DbInitializer.InitializeAsync(context, userManager, roleManager, configuration, app.Environment.IsDevelopment());
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar la base de datos.");

        // In production we fail fast: DB connectivity/migrations must be fixed before serving traffic.
        if (!app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

app.Run();
