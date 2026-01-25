using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities.Surveys;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Admin.SurveyTemplates;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly AppDbContext _context;

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList Cursos { get; set; } = null!;
    public SelectList Talleres { get; set; } = null!;

    public async Task OnGetAsync()
    {
        await LoadLookupsAsync();
        Input.IsActive = true;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadLookupsAsync();

        Input.Name = (Input.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(Input.Name))
        {
            ModelState.AddModelError("Input.Name", "El nombre es obligatorio.");
        }

        var hasCurso = Input.CursoId.HasValue;
        var hasTaller = Input.TallerId.HasValue;
        if (hasCurso == hasTaller) // both true or both false
        {
            ModelState.AddModelError(string.Empty, "Debes elegir exactamente un alcance: Curso o Taller.");
        }

        if (!ModelState.IsValid)
            return Page();

        var template = new SurveyTemplate
        {
            Name = Input.Name!,
            CursoId = Input.CursoId,
            TallerId = Input.TallerId,
            IsActive = Input.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.SurveyTemplates.Add(template);
        await _context.SaveChangesAsync();

        TempData["FlashSuccess"] = "Plantilla creada. Ahora agrega preguntas.";
        return RedirectToPage("/Admin/SurveyTemplates/Edit", new { id = template.Id });
    }

    private async Task LoadLookupsAsync()
    {
        var cursos = await _context.Cursos.OrderBy(c => c.Orden).ThenBy(c => c.Titulo).ToListAsync();
        Cursos = new SelectList(cursos, "Id", "Titulo");

        var talleres = await _context.Talleres.OrderByDescending(t => t.FechaInicio).ThenBy(t => t.Titulo).Take(300).ToListAsync();
        Talleres = new SelectList(talleres, "Id", "Titulo");
    }

    public sealed class InputModel
    {
        public string? Name { get; set; }
        public bool IsActive { get; set; } = true;
        public int? CursoId { get; set; }
        public int? TallerId { get; set; }
    }
}

