using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SumandoValor.Infrastructure.Services;

namespace SumandoValor.Web.Pages.Dev;

[Authorize]
public class EmailsModel : PageModel
{
    private readonly IDevEmailStore _emailStore;
    private readonly IWebHostEnvironment _environment;

    public EmailsModel(IDevEmailStore emailStore, IWebHostEnvironment environment)
    {
        _emailStore = emailStore;
        _environment = environment;
    }

    public List<DevEmail> Emails { get; set; } = new();

    public IActionResult OnGet()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        Emails = _emailStore.GetEmails();
        return Page();
    }
}
