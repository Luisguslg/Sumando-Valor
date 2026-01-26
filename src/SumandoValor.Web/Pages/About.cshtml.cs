using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages;

public class AboutModel : PageModel
{
    private readonly AppDbContext _context;

    public AboutModel(AppDbContext context)
    {
        _context = context;
    }

    public string? AboutImageUrl { get; set; }
    public string? AboutImageAlt { get; set; }

    public async Task OnGetAsync()
    {
        var img = await _context.SiteImages.AsNoTracking().FirstOrDefaultAsync(x => x.Key == "AboutMain");
        if (img != null)
        {
            // Use Url.Content to respect PathBase (IIS virtual directories)
            AboutImageUrl = Url.Content("~/uploads/site/" + img.FileName);
            AboutImageAlt = img.AltText;
        }
    }
}
