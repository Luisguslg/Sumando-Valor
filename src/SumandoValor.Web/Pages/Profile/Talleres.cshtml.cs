using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Profile;

[Authorize]
public class TalleresModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TalleresModel(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public List<Inscripcion> Inscripciones { get; set; } = new();

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return;
        }

        Inscripciones = await _context.Inscripciones
            .Include(i => i.Taller)
                .ThenInclude(t => t.Curso)
            .Where(i => i.UserId == user.Id && i.Estado == EstadoInscripcion.Activa)
            .OrderByDescending(i => i.Taller.FechaInicio)
            .ToListAsync();
    }
}
