using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;

namespace SumandoValor.Web.Pages.Admin;

[Authorize(Roles = "Moderador,Admin")]
public class EncuestasModel : PageModel
{
    private const int PageSize = 20;
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public EncuestasModel(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int? CursoId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? TallerId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? MinRating { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? MaxRating { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? From { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? To { get; set; }

    [BindProperty(SupportsGet = true, Name = "page")]
    public int PageNumber { get; set; } = 1;

    // For export form reusing current filter
    public FilterModel Filter => new()
    {
        CursoId = CursoId,
        TallerId = TallerId,
        Search = Search,
        MinRating = MinRating,
        MaxRating = MaxRating,
        From = From,
        To = To
    };

    public List<Curso> Cursos { get; set; } = new();
    public List<Taller> Talleres { get; set; } = new();
    public List<Row> Rows { get; set; } = new();
    public List<ResumenTaller> ResumenPorTaller { get; set; } = new();

    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public async Task OnGetAsync()
    {
        Cursos = await _context.Cursos
            .OrderBy(c => c.Orden)
            .ThenBy(c => c.Titulo)
            .ToListAsync();

        Talleres = await _context.Talleres
            .OrderByDescending(t => t.FechaInicio)
            .Take(300)
            .ToListAsync();

        var baseQuery = BuildQuery();

        // Resumen por taller (con filtro aplicado)
        ResumenPorTaller = await baseQuery
            .GroupBy(x => new { x.TallerId, x.TallerTitulo, x.CursoTitulo })
            .Select(g => new ResumenTaller
            {
                TallerId = g.Key.TallerId,
                TallerTitulo = g.Key.TallerTitulo,
                CursoTitulo = g.Key.CursoTitulo,
                Cantidad = g.Count(),
                Promedio = g.Average(x => (decimal)x.Rating1_5)
            })
            .OrderByDescending(r => r.Cantidad)
            .ThenBy(r => r.TallerTitulo)
            .Take(20)
            .ToListAsync();

        TotalCount = await baseQuery.CountAsync();

        var page = Math.Max(1, PageNumber);
        PageNumber = page;

        Rows = await baseQuery
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostExportCsvAsync([FromForm] FilterModel filter)
    {
        // Apply filters from form (not querystring)
        CursoId = filter.CursoId;
        TallerId = filter.TallerId;
        Search = filter.Search;
        MinRating = filter.MinRating;
        MaxRating = filter.MaxRating;
        From = filter.From;
        To = filter.To;

        var rows = await BuildQuery()
            .OrderByDescending(x => x.CreatedAt)
            .Take(5000)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Fecha,Usuario,Email,Taller,Curso,Rating,Comentario");
        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(",",
                EscapeCsv(r.CreatedAt.ToString("yyyy-MM-dd HH:mm")),
                EscapeCsv(r.UserFullName),
                EscapeCsv(r.UserEmail),
                EscapeCsv(r.TallerTitulo),
                EscapeCsv(r.CursoTitulo),
                EscapeCsv(r.Rating1_5.ToString()),
                EscapeCsv(r.Comentario ?? "")
            ));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv; charset=utf-8", $"encuestas_{DateTime.Now:yyyyMMdd_HHmm}.csv");
    }

    public string PageUrl(int page)
    {
        var qs = new Dictionary<string, string?>
        {
            ["page"] = page.ToString(),
            ["CursoId"] = CursoId?.ToString(),
            ["TallerId"] = TallerId?.ToString(),
            ["Search"] = Search,
            ["MinRating"] = MinRating?.ToString(),
            ["MaxRating"] = MaxRating?.ToString(),
            ["From"] = From?.ToString("yyyy-MM-dd"),
            ["To"] = To?.ToString("yyyy-MM-dd")
        };

        var parts = qs
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");

        return "/Admin/Encuestas" + (parts.Any() ? "?" + string.Join("&", parts) : "");
    }

    private IQueryable<Row> BuildQuery()
    {
        var q = from e in _context.EncuestasSatisfaccion
                join t in _context.Talleres on e.TallerId equals t.Id
                join c in _context.Cursos on t.CursoId equals c.Id
                join u in _context.Users on e.UserId equals u.Id
                select new Row
                {
                    TallerId = t.Id,
                    CursoId = c.Id,
                    TallerTitulo = t.Titulo,
                    CursoTitulo = c.Titulo,
                    UserFullName = (u.Nombres + " " + u.Apellidos).Trim(),
                    UserEmail = u.Email ?? "",
                    UserCedula = u.Cedula,
                    Rating1_5 = e.Rating1_5,
                    Comentario = e.Comentario,
                    CreatedAt = e.CreatedAt
                };

        if (CursoId.HasValue)
        {
            q = q.Where(x => x.CursoId == CursoId.Value);
        }

        if (TallerId.HasValue)
        {
            q = q.Where(x => x.TallerId == TallerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim();
            q = q.Where(x =>
                x.UserFullName.Contains(s) ||
                x.UserEmail.Contains(s) ||
                (x.UserCedula != null && x.UserCedula.Contains(s)));
        }

        if (MinRating.HasValue)
        {
            q = q.Where(x => x.Rating1_5 >= MinRating.Value);
        }

        if (MaxRating.HasValue)
        {
            q = q.Where(x => x.Rating1_5 <= MaxRating.Value);
        }

        if (From.HasValue)
        {
            var fromUtc = From.Value.Date;
            q = q.Where(x => x.CreatedAt >= fromUtc);
        }

        if (To.HasValue)
        {
            var toUtc = To.Value.Date.AddDays(1).AddTicks(-1);
            q = q.Where(x => x.CreatedAt <= toUtc);
        }

        return q;
    }

    private static string EscapeCsv(string s)
    {
        var needsQuotes = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
        if (!needsQuotes) return s;
        return "\"" + s.Replace("\"", "\"\"") + "\"";
    }

    public sealed class Row
    {
        public int TallerId { get; set; }
        public int CursoId { get; set; }
        public string TallerTitulo { get; set; } = "";
        public string CursoTitulo { get; set; } = "";
        public string UserFullName { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public string? UserCedula { get; set; }
        public int Rating1_5 { get; set; }
        public string? Comentario { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public sealed class ResumenTaller
    {
        public int TallerId { get; set; }
        public string TallerTitulo { get; set; } = "";
        public string CursoTitulo { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal Promedio { get; set; }
    }

    public sealed class FilterModel
    {
        public int? CursoId { get; set; }
        public int? TallerId { get; set; }
        public string? Search { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}

