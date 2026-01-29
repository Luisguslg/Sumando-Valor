using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SumandoValor.Domain.Helpers;
using System.Linq;
using System.Security.Claims;

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
        await SeedSuperAdminUserAsync(userManager, configuration, isDevelopment);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "Moderador", "Beneficiario" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole(roleName);
                await roleManager.CreateAsync(role);
                
                // Asignar permisos por defecto solo cuando se crea el rol
                var defaultPermissions = Permissions.GetDefaultPermissionsForRole(roleName);
                foreach (var permission in defaultPermissions)
                {
                    var claim = new Claim(Permissions.ClaimType, permission);
                    await roleManager.AddClaimAsync(role, claim);
                }
            }
        }
    }

    private static async Task SeedAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        bool isDevelopment)
    {
        var createAdminConfig = configuration["Seed:CreateAdmin"];
        var shouldCreate = isDevelopment 
            ? (string.IsNullOrWhiteSpace(createAdminConfig) || bool.TryParse(createAdminConfig, out var devB) && devB)
            : (bool.TryParse(createAdminConfig, out var prodB) && prodB);

        if (!shouldCreate)
        {
            return;
        }

        var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@sumandovalor.org";
        var adminPassword = configuration["Seed:AdminPassword"] ?? "Admin123!";

        if (!isDevelopment && (string.IsNullOrWhiteSpace(adminPassword) || adminPassword == "Admin123!"))
        {
            throw new InvalidOperationException("Seed:AdminPassword debe configurarse explícitamente en producción.");
        }

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin != null)
        {
            if (!await userManager.IsInRoleAsync(existingAdmin, "Moderador"))
            {
                await userManager.AddToRoleAsync(existingAdmin, "Moderador");
            }
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
            Sexo = "M",
            FechaNacimiento = new DateTime(1980, 1, 1),
            NivelEducativo = "UNIVERSITARIO",
            SituacionLaboral = "EMPLEADO",
            Sector = "Administración",
            CanalConocio = "ORGANIZACION",
            Pais = "Venezuela",
            Estado = "Distrito Capital",
            Municipio = "Libertador",
            CreatedAt = DateTime.UtcNow,
            EmailVerifiedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"No se pudo crear usuario Admin: {errors}");
        }

        await userManager.AddToRoleAsync(admin, "Moderador");
    }

    private static async Task SeedSuperAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        bool isDevelopment)
    {
        var create = bool.TryParse(configuration["Seed:CreateSuperAdmin"], out var b) && b;
        if (!isDevelopment && !create)
        {
            // Production safety: do not create an admin unless explicitly enabled.
            return;
        }

        if (!create && isDevelopment)
        {
            // In development, keep it opt-in as well (prevents surprising privilege changes).
            return;
        }

        var email = configuration["Seed:SuperAdminEmail"] ?? string.Empty;
        var password = configuration["Seed:SuperAdminPassword"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Seed:SuperAdminEmail debe configurarse si Seed:CreateSuperAdmin=true.");

        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Seed:SuperAdminPassword debe configurarse si Seed:CreateSuperAdmin=true.");

        if (!isDevelopment && password == "Admin123!")
        {
            throw new InvalidOperationException("Seed:SuperAdminPassword no puede ser el valor por defecto en producción.");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Nombres = "Administrador",
                Apellidos = "Sistema",
                Cedula = "00000001",
                Sexo = "M",
                FechaNacimiento = new DateTime(1980, 1, 1),
                NivelEducativo = "UNIVERSITARIO",
                SituacionLaboral = "EMPLEADO",
                Sector = "Administración",
                CanalConocio = "ORGANIZACION",
                Pais = "Venezuela",
                Estado = "Distrito Capital",
                Municipio = "Libertador",
                CreatedAt = DateTime.UtcNow,
                EmailVerifiedAt = DateTime.UtcNow
            };

            var createRes = await userManager.CreateAsync(user, password);
            if (!createRes.Succeeded)
            {
                var msg = string.Join("; ", createRes.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"No se pudo crear Admin: {msg}");
            }
        }

        // Asegurar rol Admin
        if (!await userManager.IsInRoleAsync(user, "Admin"))
            await userManager.AddToRoleAsync(user, "Admin");
    }
}
