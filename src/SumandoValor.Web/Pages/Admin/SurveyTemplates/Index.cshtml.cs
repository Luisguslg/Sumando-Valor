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
                t.Description,
                QuestionCount = t.Questions.Count
            })
            .ToListAsync();

        Rows = templates.Select(t => new Row
        {
            Id = t.Id,
            Name = t.Name,
            IsActive = t.IsActive,
            Description = t.Description,
            QuestionCount = t.QuestionCount
        }).ToList();
    }

    public sealed class Row
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public int QuestionCount { get; set; }
    }
}

