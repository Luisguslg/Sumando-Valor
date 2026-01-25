using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities.Surveys;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Admin.SurveyTemplates;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly AppDbContext _context;

    public EditModel(AppDbContext context)
    {
        _context = context;
    }

    public SurveyTemplate? Template { get; set; }
    public string ScopeText { get; set; } = "";
    public List<QuestionRow> Questions { get; set; } = new();

    public async Task OnGetAsync(int id)
    {
        await LoadAsync(id);
    }

    public async Task<IActionResult> OnPostUpdateTemplateAsync(int id, string name, bool? isActive)
    {
        var template = await _context.SurveyTemplates.FirstOrDefaultAsync(t => t.Id == id);
        if (template == null) return RedirectToPage("/Admin/SurveyTemplates/Index");

        template.Name = (name ?? string.Empty).Trim();
        template.IsActive = isActive == true;
        await _context.SaveChangesAsync();
        TempData["FlashSuccess"] = "Plantilla actualizada.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAddQuestionAsync(int id, SurveyQuestionType type, string text, bool? isRequired, string? options)
    {
        var template = await _context.SurveyTemplates.Include(t => t.Questions).FirstOrDefaultAsync(t => t.Id == id);
        if (template == null) return RedirectToPage("/Admin/SurveyTemplates/Index");

        text = (text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            TempData["FlashError"] = "El texto de la pregunta es obligatorio.";
            return RedirectToPage(new { id });
        }

        string optionsJson = string.Empty;
        if (type == SurveyQuestionType.SingleChoice)
        {
            var lines = (options ?? string.Empty)
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(25)
                .ToList();

            if (lines.Count < 2)
            {
                TempData["FlashError"] = "Para “Opción única” debes indicar al menos 2 opciones (una por línea).";
                return RedirectToPage(new { id });
            }

            optionsJson = JsonSerializer.Serialize(lines);
        }

        var nextOrder = (template.Questions.Max(q => (int?)q.Order) ?? 0) + 1;
        _context.SurveyQuestions.Add(new SurveyQuestion
        {
            TemplateId = id,
            Type = type,
            Text = text,
            Order = nextOrder,
            IsRequired = isRequired == true,
            OptionsJson = optionsJson
        });
        await _context.SaveChangesAsync();
        TempData["FlashSuccess"] = "Pregunta agregada.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteQuestionAsync(int id, int questionId)
    {
        var q = await _context.SurveyQuestions.FirstOrDefaultAsync(x => x.Id == questionId && x.TemplateId == id);
        if (q != null)
        {
            _context.SurveyQuestions.Remove(q);
            await _context.SaveChangesAsync();
            TempData["FlashSuccess"] = "Pregunta eliminada.";
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostMoveQuestionUpAsync(int id, int questionId)
    {
        var qs = await _context.SurveyQuestions.Where(x => x.TemplateId == id).OrderBy(x => x.Order).ThenBy(x => x.Id).ToListAsync();
        var idx = qs.FindIndex(x => x.Id == questionId);
        if (idx > 0)
        {
            (qs[idx - 1].Order, qs[idx].Order) = (qs[idx].Order, qs[idx - 1].Order);
            await _context.SaveChangesAsync();
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostMoveQuestionDownAsync(int id, int questionId)
    {
        var qs = await _context.SurveyQuestions.Where(x => x.TemplateId == id).OrderBy(x => x.Order).ThenBy(x => x.Id).ToListAsync();
        var idx = qs.FindIndex(x => x.Id == questionId);
        if (idx >= 0 && idx < qs.Count - 1)
        {
            (qs[idx + 1].Order, qs[idx].Order) = (qs[idx].Order, qs[idx + 1].Order);
            await _context.SaveChangesAsync();
        }
        return RedirectToPage(new { id });
    }

    private async Task LoadAsync(int id)
    {
        Template = await _context.SurveyTemplates
            .Include(t => t.Questions)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (Template == null)
            return;

        ScopeText =
            Template.TallerId.HasValue ? $"TallerId={Template.TallerId}" :
            Template.CursoId.HasValue ? $"CursoId={Template.CursoId}" :
            "Sin alcance";

        var ordered = Template.Questions.OrderBy(q => q.Order).ThenBy(q => q.Id).ToList();
        Questions = ordered.Select((q, idx) => new QuestionRow
        {
            Id = q.Id,
            Type = q.Type,
            Text = q.Text,
            Order = q.Order,
            IsRequired = q.IsRequired,
            CanMoveUp = idx > 0,
            CanMoveDown = idx < ordered.Count - 1
        }).ToList();
    }

    public sealed class QuestionRow
    {
        public int Id { get; set; }
        public SurveyQuestionType Type { get; set; }
        public string Text { get; set; } = "";
        public int Order { get; set; }
        public bool IsRequired { get; set; }
        public bool CanMoveUp { get; set; }
        public bool CanMoveDown { get; set; }
    }
}

