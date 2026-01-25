using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Domain.Entities.Surveys;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Profile;

[Authorize(Roles = "Beneficiario")]
public class EncuestaModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public EncuestaModel(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int TallerId { get; set; }

    [BindProperty]
    public int TemplateId { get; set; }

    [BindProperty]
    public Dictionary<int, string> Answers { get; set; } = new();

    public Taller? Taller { get; set; }
    public SurveyTemplate? Template { get; set; }
    public List<QuestionVm> Questions { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        Taller = await _context.Talleres.Include(t => t.Curso).FirstOrDefaultAsync(t => t.Id == TallerId);
        if (Taller == null) return Page();

        if (Taller.Estatus != EstatusTaller.Finalizado || !Taller.RequiereEncuesta)
            return Page();

        var inscrito = await _context.Inscripciones.AnyAsync(i => i.TallerId == TallerId && i.UserId == user.Id && i.Estado == EstadoInscripcion.Activa);
        if (!inscrito) return Forbid();

        var already = await _context.EncuestasSatisfaccion.AnyAsync(e => e.TallerId == TallerId && e.UserId == user.Id);
        if (already)
        {
            TempData["FlashInfo"] = "Ya respondiste la encuesta de este taller.";
            return RedirectToPage("/Profile/Talleres");
        }

        Template = await ResolveTemplateAsync(Taller);
        if (Template == null) return Page();

        TemplateId = Template.Id;
        Questions = Template.Questions
            .OrderBy(q => q.Order)
            .ThenBy(q => q.Id)
            .Select(q => new QuestionVm(q))
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        Taller = await _context.Talleres.Include(t => t.Curso).FirstOrDefaultAsync(t => t.Id == TallerId);
        if (Taller == null) return NotFound();

        if (Taller.Estatus != EstatusTaller.Finalizado || !Taller.RequiereEncuesta)
        {
            TempData["FlashError"] = "La encuesta no está disponible para este taller.";
            return RedirectToPage("/Profile/Talleres");
        }

        var inscrito = await _context.Inscripciones.AnyAsync(i => i.TallerId == TallerId && i.UserId == user.Id && i.Estado == EstadoInscripcion.Activa);
        if (!inscrito) return Forbid();

        var already = await _context.EncuestasSatisfaccion.AnyAsync(e => e.TallerId == TallerId && e.UserId == user.Id);
        if (already)
        {
            TempData["FlashInfo"] = "Ya respondiste la encuesta de este taller.";
            return RedirectToPage("/Profile/Talleres");
        }

        Template = await _context.SurveyTemplates
            .Include(t => t.Questions)
            .FirstOrDefaultAsync(t => t.Id == TemplateId && t.IsActive);

        if (Template == null)
        {
            TempData["FlashError"] = "Plantilla no disponible.";
            return RedirectToPage("/Profile/Talleres");
        }

        var orderedQuestions = Template.Questions.OrderBy(q => q.Order).ThenBy(q => q.Id).ToList();
        Questions = orderedQuestions.Select(q => new QuestionVm(q)).ToList();

        // Validate answers
        foreach (var q in orderedQuestions)
        {
            Answers.TryGetValue(q.Id, out var val);
            val = (val ?? string.Empty).Trim();

            if (q.IsRequired && string.IsNullOrWhiteSpace(val))
            {
                TempData["FlashError"] = "Completa las preguntas obligatorias.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(val))
                continue;

            if (q.Type == SurveyQuestionType.Rating1To5)
            {
                if (!int.TryParse(val, out var r) || r < 1 || r > 5)
                {
                    TempData["FlashError"] = "Hay una calificación inválida (debe ser 1 a 5).";
                    return Page();
                }
            }
            else if (q.Type == SurveyQuestionType.SingleChoice)
            {
                var opts = QuestionVm.ParseOptions(q.OptionsJson);
                if (opts.Count > 0 && !opts.Contains(val, StringComparer.OrdinalIgnoreCase))
                {
                    TempData["FlashError"] = "Hay una opción inválida en la encuesta.";
                    return Page();
                }
            }
        }

        // Save response
        var response = new SurveyResponse
        {
            TemplateId = Template.Id,
            TallerId = TallerId,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var q in orderedQuestions)
        {
            Answers.TryGetValue(q.Id, out var val);
            val = (val ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(val))
                continue;

            response.Answers.Add(new SurveyAnswer
            {
                QuestionId = q.Id,
                Value = val
            });
        }

        _context.SurveyResponses.Add(response);

        // Compatibility: also create EncuestaSatisfaccion so certificate logic remains unchanged.
        var ratings = orderedQuestions
            .Where(q => q.Type == SurveyQuestionType.Rating1To5)
            .Select(q => Answers.TryGetValue(q.Id, out var v) && int.TryParse(v, out var r) ? (int?)r : null)
            .Where(r => r.HasValue)
            .Select(r => r!.Value)
            .ToList();

        var score = ratings.Count > 0 ? ratings.Average() : 0;
        var comment = orderedQuestions.FirstOrDefault(q => q.Type == SurveyQuestionType.Text) is { } textQ
            ? (Answers.TryGetValue(textQ.Id, out var c) ? c?.Trim() : null)
            : null;

        var payload = JsonSerializer.Serialize(new
        {
            templateId = Template.Id,
            templateName = Template.Name,
            answers = response.Answers.Select(a => new { a.QuestionId, a.Value })
        });

        _context.EncuestasSatisfaccion.Add(new EncuestaSatisfaccion
        {
            TallerId = TallerId,
            UserId = user.Id,
            Rating1_5 = ratings.Count > 0 ? (int)Math.Round(score, MidpointRounding.AwayFromZero) : 0,
            Comentario = string.IsNullOrWhiteSpace(comment) ? null : comment,
            ScorePromedio = ratings.Count > 0 ? (decimal)score : null,
            PayloadJson = payload,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        TempData["FlashSuccess"] = "¡Gracias! Tu encuesta fue enviada correctamente.";
        return RedirectToPage("/Profile/Talleres");
    }

    private async Task<SurveyTemplate?> ResolveTemplateAsync(Taller taller)
    {
        // Prefer a template bound to this Taller, else a template bound to the Curso.
        var byTaller = await _context.SurveyTemplates
            .Include(t => t.Questions)
            .Where(t => t.IsActive && t.TallerId == taller.Id)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();
        if (byTaller != null) return byTaller;

        var byCurso = await _context.SurveyTemplates
            .Include(t => t.Questions)
            .Where(t => t.IsActive && t.CursoId == taller.CursoId && t.TallerId == null)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();
        return byCurso;
    }

    public sealed class QuestionVm
    {
        public QuestionVm(SurveyQuestion q)
        {
            Id = q.Id;
            Type = q.Type;
            Text = q.Text;
            IsRequired = q.IsRequired;
            Options = ParseOptions(q.OptionsJson);
        }

        public int Id { get; }
        public SurveyQuestionType Type { get; }
        public string Text { get; }
        public bool IsRequired { get; }
        public List<string> Options { get; }

        public static List<string> ParseOptions(string? optionsJson)
        {
            if (string.IsNullOrWhiteSpace(optionsJson))
                return new List<string>();
            try
            {
                return JsonSerializer.Deserialize<List<string>>(optionsJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}

