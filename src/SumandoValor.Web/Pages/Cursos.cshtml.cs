using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace SumandoValor.Web.Pages;

public class CursosModel : PageModel
{
    private readonly AppDbContext _context;

    public CursosModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Curso> Cursos { get; set; } = new();
    public Dictionary<int, bool> CursosConAcceso { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Mostrar TODOS los cursos activos (públicos e internos)
        // Los internos aparecen para que el usuario pueda acceder con clave si la tiene
        Cursos = await _context.Cursos
            .Where(c => c.Estado == EstatusCurso.Activo)
            .OrderBy(c => c.Orden)
            .ThenBy(c => c.Titulo)
            .ToListAsync();

        // Verificar acceso para cada curso interno
        foreach (var curso in Cursos)
        {
            if (curso.EsPublico)
            {
                CursosConAcceso[curso.Id] = true; // Públicos siempre tienen acceso
            }
            else
            {
                // Verificar acceso en sesión
                var hasAccess = HttpContext.Session.GetString($"curso_access_{curso.Id}") == "granted";
                
                // Si no hay acceso en sesión, verificar cookie
                if (!hasAccess)
                {
                    var cookieToken = Request.Cookies[$"curso_token_{curso.Id}"];
                    if (!string.IsNullOrEmpty(cookieToken) &&
                        !string.IsNullOrEmpty(curso.TokenAccesoUnico) &&
                        curso.TokenAccesoUnico.Equals(cookieToken, StringComparison.Ordinal) &&
                        (curso.TokenExpiracion == null || curso.TokenExpiracion > DateTime.UtcNow))
                    {
                        // Restaurar acceso en sesión desde cookie
                        HttpContext.Session.SetString($"curso_access_{curso.Id}", "granted");
                        hasAccess = true;
                    }
                }
                
                CursosConAcceso[curso.Id] = hasAccess;
            }
        }
    }
}
