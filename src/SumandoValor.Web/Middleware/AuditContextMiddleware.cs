using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Middleware;

public class AuditContextMiddleware
{
    private readonly RequestDelegate _next;

    public AuditContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user != null)
            {
                await dbContext.Database.ExecuteSqlRawAsync(
                    "EXEC sp_set_session_context @key = N'UserId', @value = {0}, @readonly = 0",
                    user.Id);
                await dbContext.Database.ExecuteSqlRawAsync(
                    "EXEC sp_set_session_context @key = N'UserEmail', @value = {0}, @readonly = 0",
                    user.Email ?? string.Empty);
            }
        }

        await _next(context);
    }
}
