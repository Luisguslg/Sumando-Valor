using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Infrastructure.Data;
using SumandoValor.Infrastructure.Services;
using SumandoValor.Web.Services.Certificates;

namespace SumandoValor.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class CertificadosModel : PageModel
{
    private const int PageSize = 20;
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _env;
    private readonly CertificatePdfGenerator _pdf;
    private readonly ILogger<CertificadosModel> _logger;

    public CertificadosModel(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IEmailService emailService,
        IWebHostEnvironment env,
        CertificatePdfGenerator pdf,
        ILogger<CertificadosModel> logger)
    {
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
        _emailService = emailService;
        _env = env;
        _pdf = pdf;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public int? TallerId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Estado { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true, Name = "page")]
    public int PageNumber { get; set; } = 1;

    public List<Taller> Talleres { get; set; } = new();
    public List<Row> Rows { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    [BindProperty]
    public List<int> SelectedInscripcionIds { get; set; } = new();

    public async Task OnGetAsync()
    {
        Talleres = await _context.Talleres
            .Where(t => t.PermiteCertificado)
            .OrderByDescending(t => t.FechaInicio)
            .Take(300)
            .ToListAsync();

        var q = BuildBaseQuery();
        TotalCount = await q.CountAsync();

        var page = Math.Max(1, PageNumber);
        PageNumber = page;

        Rows = await q
            .OrderByDescending(x => x.TallerFechaInicio)
            .ThenBy(x => x.UserFullName)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostApproveSelectedAsync()
    {
        if (SelectedInscripcionIds.Count == 0)
        {
            TempData["FlashInfo"] = "Selecciona al menos una fila.";
            return RedirectToPage("/Admin/Certificados", new { TallerId, Estado, Search, page = PageNumber });
        }

        var sendEmail = bool.TryParse(_configuration["Email:SendCertificateNotification"], out var v) && v;
        var certDir = Path.Combine(_env.ContentRootPath, "App_Data", "Certificates");
        Directory.CreateDirectory(certDir);

        var inscripciones = await _context.Inscripciones
            .Include(i => i.Taller).ThenInclude(t => t.Curso)
            .Where(i => SelectedInscripcionIds.Contains(i.Id))
            .ToListAsync();

        var userIds = inscripciones.Select(i => i.UserId).Distinct().ToList();
        var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id);

        var tallerIds = inscripciones.Select(i => i.TallerId).Distinct().ToList();
        var encuestas = await _context.EncuestasSatisfaccion
            .Where(e => userIds.Contains(e.UserId) && tallerIds.Contains(e.TallerId))
            .ToListAsync();

        var encuestasSet = encuestas.ToHashSetBy(x => (x.TallerId, x.UserId));

        int approved = 0;
        int skipped = 0;

        foreach (var ins in inscripciones)
        {
            if (!users.TryGetValue(ins.UserId, out var user))
            {
                skipped++;
                continue;
            }

            if (!IsEligible(ins, encuestasSet))
            {
                skipped++;
                continue;
            }

            var cert = await _context.Certificados.FirstOrDefaultAsync(c => c.TallerId == ins.TallerId && c.UserId == ins.UserId);
            if (cert == null)
            {
                cert = new Certificado
                {
                    TallerId = ins.TallerId,
                    UserId = ins.UserId,
                    Estado = EstadoCertificado.Aprobado,
                    CreatedAt = DateTime.UtcNow,
                    IssuedAt = DateTime.UtcNow
                };
                _context.Certificados.Add(cert);
                await _context.SaveChangesAsync(); // need Id for filename
            }
            else
            {
                cert.Estado = EstadoCertificado.Aprobado;
                cert.IssuedAt = DateTime.UtcNow;
            }

            var pdfBytes = _pdf.Generate(new CertificatePdfData
            {
                NombreCompleto = $"{user.Nombres} {user.Apellidos}".Trim(),
                TallerTitulo = ins.Taller.Titulo,
                DuracionTexto = FormatDuration(ins.Taller.FechaInicio, ins.Taller.FechaFin),
                Fecha = ins.Taller.FechaFin ?? ins.Taller.FechaInicio
            });

            // FIX: never overwrite PDFs. Generate a unique filename per issuance and (optionally) delete the previous file.
            if (!string.IsNullOrWhiteSpace(cert.UrlPdf))
            {
                try
                {
                    var oldNormalized = cert.UrlPdf.Replace('/', Path.DirectorySeparatorChar);
                    var oldPhysical = Path.Combine(_env.ContentRootPath, oldNormalized);
                    if (System.IO.File.Exists(oldPhysical))
                    {
                        System.IO.File.Delete(oldPhysical);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo eliminar PDF anterior del certificado. CertId={CertId}", cert.Id);
                }
            }

            string relative;
            string physical;
            do
            {
                var fileName = $"cert_{cert.Id}_{ins.TallerId}_{ins.UserId}_{Guid.NewGuid():N}.pdf";
                relative = Path.Combine("App_Data", "Certificates", fileName);
                physical = Path.Combine(_env.ContentRootPath, relative);
            } while (System.IO.File.Exists(physical));

            await System.IO.File.WriteAllBytesAsync(physical, pdfBytes);

            cert.UrlPdf = relative.Replace(Path.DirectorySeparatorChar, '/');

            await _context.SaveChangesAsync();
            approved++;

            if (sendEmail && !string.IsNullOrWhiteSpace(user.Email))
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        user.Email,
                        "Tu certificado está disponible - Sumando Valor",
                        $"¡Hola {user.Nombres}!\n\nTu certificado del taller \"{ins.Taller.Titulo}\" ya está disponible en la plataforma (Mis Talleres).\n");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo enviar notificación de certificado. CertId={CertId}, Email={Email}", cert.Id, user.Email);
                }
            }
        }

        TempData["FlashSuccess"] = $"Certificados aprobados: {approved}. Omitidos: {skipped}.";
        return RedirectToPage("/Admin/Certificados", new { TallerId, Estado, Search, page = PageNumber });
    }

    public async Task<IActionResult> OnPostRevokeSelectedAsync()
    {
        if (SelectedInscripcionIds.Count == 0)
        {
            TempData["FlashInfo"] = "Selecciona al menos una fila.";
            return RedirectToPage("/Admin/Certificados", new { TallerId, Estado, Search, page = PageNumber });
        }

        var inscripciones = await _context.Inscripciones
            .Where(i => SelectedInscripcionIds.Contains(i.Id))
            .ToListAsync();

        int revoked = 0;
        foreach (var ins in inscripciones)
        {
            var cert = await _context.Certificados.FirstOrDefaultAsync(c => c.TallerId == ins.TallerId && c.UserId == ins.UserId);
            if (cert == null)
                continue;

            cert.Estado = EstadoCertificado.Rechazado;
            cert.IssuedAt = null;

            if (!string.IsNullOrWhiteSpace(cert.UrlPdf))
            {
                var normalized = cert.UrlPdf.Replace('/', Path.DirectorySeparatorChar);
                var physical = Path.Combine(_env.ContentRootPath, normalized);
                if (System.IO.File.Exists(physical))
                {
                    try { System.IO.File.Delete(physical); } catch { /* ignore */ }
                }

                cert.UrlPdf = null;
            }

            revoked++;
        }

        await _context.SaveChangesAsync();
        TempData["FlashSuccess"] = $"Certificados revocados: {revoked}.";
        return RedirectToPage("/Admin/Certificados", new { TallerId, Estado, Search, page = PageNumber });
    }

    public string PageUrl(int page)
    {
        var qs = new Dictionary<string, string?>
        {
            ["page"] = page.ToString(),
            ["TallerId"] = TallerId?.ToString(),
            ["Estado"] = Estado?.ToString(),
            ["Search"] = Search
        };

        var parts = qs.Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");
        return "/Admin/Certificados" + (parts.Any() ? "?" + string.Join("&", parts) : "");
    }

    private IQueryable<Row> BuildBaseQuery()
    {
        var q = from i in _context.Inscripciones
                join t in _context.Talleres on i.TallerId equals t.Id
                join c in _context.Cursos on t.CursoId equals c.Id
                join u in _context.Users on i.UserId equals u.Id
                join e in _context.EncuestasSatisfaccion on new { i.TallerId, i.UserId } equals new { e.TallerId, e.UserId } into encJoin
                from e in encJoin.DefaultIfEmpty()
                join cert in _context.Certificados on new { i.TallerId, i.UserId } equals new { cert.TallerId, cert.UserId } into certJoin
                from cert in certJoin.DefaultIfEmpty()
                where i.Estado == EstadoInscripcion.Activa && t.PermiteCertificado
                select new Row
                {
                    InscripcionId = i.Id,
                    TallerId = t.Id,
                    TallerTitulo = t.Titulo,
                    CursoTitulo = c.Titulo,
                    TallerFechaInicio = t.FechaInicio,
                    TallerEstatus = t.Estatus,
                    EncuestaRequired = t.RequiereEncuesta,
                    UserId = u.Id,
                    UserFullName = (u.Nombres + " " + u.Apellidos).Trim(),
                    UserEmail = u.Email ?? "",
                    UserCedula = u.Cedula,
                    AsistenciaOk = i.Asistencia,
                    EncuestaOk = e != null,
                    CertificadoId = cert != null ? cert.Id : (int?)null,
                    Estado = cert != null ? cert.Estado : (EstadoCertificado?)null,
                    HasPdf = cert != null && cert.UrlPdf != null
                };

        if (TallerId.HasValue)
            q = q.Where(x => x.TallerId == TallerId.Value);

        if (Estado.HasValue)
            q = q.Where(x => (int?)x.Estado == Estado.Value);

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim();
            q = q.Where(x =>
                x.UserFullName.Contains(s) ||
                x.UserEmail.Contains(s) ||
                (x.UserCedula != null && x.UserCedula.Contains(s)));
        }

        return q;
    }

    private static bool IsEligible(Inscripcion ins, HashSet<(int TallerId, string UserId)> encuestaSet)
    {
        if (ins.Taller.PermiteCertificado == false)
            return false;
        if (ins.Taller.Estatus != EstatusTaller.Finalizado)
            return false;
        if (!ins.Asistencia)
            return false;
        if (ins.Taller.RequiereEncuesta && !encuestaSet.Contains((ins.TallerId, ins.UserId)))
            return false;
        return true;
    }

    private static string FormatDuration(DateTime start, DateTime? end)
    {
        if (end == null)
            return "—";

        var diff = end.Value - start;
        if (diff.TotalMinutes <= 0)
            return "—";

        if (diff.TotalHours < 1)
            return $"{Math.Round(diff.TotalMinutes)} min";

        if (diff.TotalHours < 24)
            return $"{Math.Round(diff.TotalHours, 1):0.#} h";

        var days = (int)Math.Floor(diff.TotalDays);
        var hours = Math.Round(diff.TotalHours - (days * 24), 1);
        if (hours <= 0.1)
            return $"{days} d";
        return $"{days} d {hours:0.#} h";
    }

    public sealed class Row
    {
        public int InscripcionId { get; set; }
        public int TallerId { get; set; }
        public string TallerTitulo { get; set; } = "";
        public string CursoTitulo { get; set; } = "";
        public DateTime TallerFechaInicio { get; set; }
        public EstatusTaller TallerEstatus { get; set; }
        public bool EncuestaRequired { get; set; }

        public string UserId { get; set; } = "";
        public string UserFullName { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public string? UserCedula { get; set; }

        public bool AsistenciaOk { get; set; }
        public bool EncuestaOk { get; set; }

        public int? CertificadoId { get; set; }
        public EstadoCertificado? Estado { get; set; }
        public bool HasPdf { get; set; }

        public bool Eligible =>
            TallerEstatus == EstatusTaller.Finalizado &&
            AsistenciaOk &&
            (!EncuestaRequired || EncuestaOk);

        public bool CanSelect => Eligible || CertificadoId.HasValue;

        public string EstadoText =>
            Estado?.ToString() ??
            (Eligible ? "Pendiente" : "No elegible");

        public string EstadoBadgeClass => Estado switch
        {
            EstadoCertificado.Aprobado => "bg-success",
            EstadoCertificado.Rechazado => "bg-danger",
            _ => "bg-info"
        };
    }
}

internal static class HashSetExtensions
{
    public static HashSet<TKey> ToHashSetBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> keySelector)
        where TKey : notnull
    {
        var set = new HashSet<TKey>();
        foreach (var item in items)
            set.Add(keySelector(item));
        return set;
    }
}

