using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages;

public class CursosModel : PageModel
{
    private readonly AppDbContext _context;

    public CursosModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Curso> Cursos { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Solo mostrar cursos públicos en la vista pública
        Cursos = await _context.Cursos
            .Where(c => c.Estado == EstatusCurso.Activo && c.EsPublico)
            .OrderBy(c => c.Orden)
            .ThenBy(c => c.Titulo)
            .ToListAsync();
    }
}
