using Microsoft.AspNetCore.Mvc;
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
        var todosCursos = await _context.Cursos
            .Where(c => c.Estado == EstatusCurso.Activo)
            .OrderBy(c => c.Orden)
            .ThenBy(c => c.Titulo)
            .ToListAsync();

        // Filtrar: solo mostrar públicos + internos con acceso
        var cursosVisibles = new List<Curso>();

        foreach (var curso in todosCursos)
        {
            if (curso.EsPublico)
            {
                // Curso público: siempre visible
                cursosVisibles.Add(curso);
                CursosConAcceso[curso.Id] = true;
            }
            else
            {
                // Curso interno: verificar acceso
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
                
                // Solo agregar si tiene acceso
                if (hasAccess)
                {
                    cursosVisibles.Add(curso);
                    CursosConAcceso[curso.Id] = true;
                }
            }
        }

        Cursos = cursosVisibles;
    }

    public async Task<IActionResult> OnPostValidateCodeAsync(string codigoAcceso)
    {
        if (string.IsNullOrWhiteSpace(codigoAcceso))
        {
            TempData["FlashError"] = "Por favor ingresa un código de acceso.";
            return RedirectToPage();
        }

        // Buscar curso por ClaveAcceso (EF no traduce StringComparison, filtramos en memoria)
        var codigoTrimmed = codigoAcceso.Trim();
        var cursosCandidatos = await _context.Cursos
            .Where(c => c.Estado == EstatusCurso.Activo && !c.EsPublico && c.ClaveAcceso != null && c.ClaveAcceso != "")
            .ToListAsync();

        var curso = cursosCandidatos
            .FirstOrDefault(c => c.ClaveAcceso!.Equals(codigoTrimmed, StringComparison.OrdinalIgnoreCase));

        if (curso == null)
        {
            TempData["FlashError"] = "El código de acceso ingresado no es válido o no corresponde a ningún programa formativo activo.";
            return RedirectToPage();
        }

        // Otorgar acceso
        HttpContext.Session.SetString($"curso_access_{curso.Id}", "granted");
        
        // Si tiene TokenAccesoUnico, guardarlo en cookie también
        if (!string.IsNullOrEmpty(curso.TokenAccesoUnico))
        {
            HttpContext.Response.Cookies.Append($"curso_token_{curso.Id}", curso.TokenAccesoUnico, new CookieOptions
            {
                HttpOnly = true,
                Secure = !Request.IsHttps ? false : true,
                SameSite = SameSiteMode.Lax,
                Expires = curso.TokenExpiracion ?? DateTimeOffset.UtcNow.AddYears(1)
            });
        }

        TempData["FlashSuccess"] = $"Acceso otorgado al programa formativo: {curso.Titulo}";
        return RedirectToPage();
    }
}
