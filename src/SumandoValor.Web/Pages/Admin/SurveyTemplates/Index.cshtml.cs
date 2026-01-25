using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities.Surveys;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Admin.SurveyTemplates;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Row> Rows { get; set; } = new();

    public async Task OnGetAsync()
    {
        var templates = await _context.SurveyTemplates
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.IsActive,
                t.CursoId,
                t.TallerId,
                QuestionCount = t.Questions.Count
            })
            .ToListAsync();

        var cursoIds = templates.Where(t => t.CursoId.HasValue).Select(t => t.CursoId!.Value).Distinct().ToList();
        var tallerIds = templates.Where(t => t.TallerId.HasValue).Select(t => t.TallerId!.Value).Distinct().ToList();

        var cursos = await _context.Cursos.Where(c => cursoIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Titulo);
        var talleres = await _context.Talleres.Where(t => tallerIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id, t => t.Titulo);

        Rows = templates.Select(t => new Row
        {
            Id = t.Id,
            Name = t.Name,
            IsActive = t.IsActive,
            CursoId = t.CursoId,
            TallerId = t.TallerId,
            CursoTitulo = t.CursoId.HasValue && cursos.TryGetValue(t.CursoId.Value, out var ct) ? ct : null,
            TallerTitulo = t.TallerId.HasValue && talleres.TryGetValue(t.TallerId.Value, out var tt) ? tt : null,
            QuestionCount = t.QuestionCount
        }).ToList();
    }

    public sealed class Row
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; }
        public int? CursoId { get; set; }
        public int? TallerId { get; set; }
        public string? CursoTitulo { get; set; }
        public string? TallerTitulo { get; set; }
        public int QuestionCount { get; set; }

        public string ScopeText =>
            TallerId.HasValue ? $"Taller: {TallerTitulo ?? TallerId!.Value.ToString()}" :
            CursoId.HasValue ? $"Programa formativo: {CursoTitulo ?? CursoId!.Value.ToString()}" :
            "Sin alcance";
    }
}

