using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities.Surveys;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Admin.SurveyTemplates;

[Authorize(Roles = "Moderador,Admin")]
public class CreateModel : PageModel
{
    private readonly AppDbContext _context;

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public void OnGet()
    {
        Input.IsActive = true;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Input.Name = (Input.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(Input.Name))
        {
            ModelState.AddModelError("Input.Name", "El nombre es obligatorio.");
        }
        
        Input.Description = (Input.Description ?? string.Empty).Trim();

        if (!ModelState.IsValid)
            return Page();

        // If creating as active, deactivate any other active survey (keep UX simple: one global survey).
        if (Input.IsActive)
        {
            var actives = _context.SurveyTemplates.Where(t => t.IsActive);
            await actives.ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false));
        }

        var template = new SurveyTemplate
        {
            Name = Input.Name!,
            Description = Input.Description ?? string.Empty,
            IsActive = Input.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.SurveyTemplates.Add(template);
        await _context.SaveChangesAsync();

        TempData["FlashSuccess"] = "Plantilla creada. Ahora agrega preguntas.";
        return RedirectToPage("/Admin/SurveyTemplates/Edit", new { id = template.Id });
    }

    public sealed class InputModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

