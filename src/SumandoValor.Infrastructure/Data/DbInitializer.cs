using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SumandoValor.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        bool isDevelopment)
    {
        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager, configuration, isDevelopment);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "Beneficiario" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private static async Task SeedAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        bool isDevelopment)
    {
        var createAdmin = bool.TryParse(configuration["Seed:CreateAdmin"], out var b) && b;
        if (!isDevelopment && !createAdmin)
        {
            // Production safety: do not create an admin unless explicitly enabled.
            return;
        }

        var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@sumandovalor.org";
        var adminPassword = configuration["Seed:AdminPassword"] ?? "Admin123!";

        if (!isDevelopment && (string.IsNullOrWhiteSpace(adminPassword) || adminPassword == "Admin123!"))
        {
            throw new InvalidOperationException("Seed:AdminPassword debe configurarse explícitamente en producción (no se permite el valor por defecto).");
        }

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin != null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            Nombres = "Administrador",
            Apellidos = "Sistema",
            Cedula = "00000000",
            CreatedAt = DateTime.UtcNow,
            EmailVerifiedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
