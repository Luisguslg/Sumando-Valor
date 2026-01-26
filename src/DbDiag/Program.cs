using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SumandoValor.Infrastructure.Data;
using SumandoValor.Domain.Entities;

// Find repository root (where SumandoValor.sln exists)
string? FindRepoRoot()
{
    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (dir != null)
    {
        if (File.Exists(Path.Combine(dir.FullName, "SumandoValor.sln")))
            return dir.FullName;
        dir = dir.Parent;
    }
    return null;
}

var repoRoot = FindRepoRoot();
if (repoRoot == null)
{
    Console.WriteLine("No se encontró SumandoValor.sln en los padres del directorio actual. Pasa la cadena de conexión con la variable de entorno 'ConnectionStrings__DefaultConnection'.");
}

var builder = new ConfigurationBuilder();
// Prefer environment override
builder.AddEnvironmentVariables();

// Try Web appsettings
if (repoRoot != null)
{
    var webDev = Path.Combine(repoRoot, "src", "SumandoValor.Web", "appsettings.Development.json");
    var webApp = Path.Combine(repoRoot, "src", "SumandoValor.Web", "appsettings.json");
    if (File.Exists(webDev)) builder.AddJsonFile(webDev, optional: true, reloadOnChange: false);
    if (File.Exists(webApp)) builder.AddJsonFile(webApp, optional: true, reloadOnChange: false);
}

var config = builder.Build();
var conn = config.GetConnectionString("DefaultConnection");
// Allow an explicit override for diagnostics to avoid touching remote/production DB.
var forced = Environment.GetEnvironmentVariable("DIAG_FORCE_CONN");
if (!string.IsNullOrWhiteSpace(forced))
{
    conn = forced;
    Console.WriteLine("Usando override DIAG_FORCE_CONN (diagnóstico).\n");
}
if (string.IsNullOrWhiteSpace(conn))
{
    Console.WriteLine("No se encontró la cadena de conexión en archivos. Revisa variable de entorno 'ConnectionStrings__DefaultConnection'.");
    return 1;
}

var services = new ServiceCollection();
Console.WriteLine($"Usando cadena de conexión: {conn}\n");
// Try connect using configured conn; if it fails, attempt some local fallbacks (best-effort).
var tried = new List<string>();
Exception? lastEx = null;
string? selectedConn = null;
var candidates = new List<string> { conn, "Server=localhost;Database=SumandoValorDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;", "Server=(localdb)\\MSSQLLocalDB;Database=SumandoValorDb;Trusted_Connection=True;" };
foreach (var candidate in candidates.Where(s => !string.IsNullOrWhiteSpace(s)))
{
    tried.Add(candidate);
    try
    {
        var servicesTry = new ServiceCollection();
        servicesTry.AddDbContext<AppDbContext>(o => o.UseSqlServer(candidate));
        using var spTry = servicesTry.BuildServiceProvider();
        using var scopeTry = spTry.CreateScope();
        var ctxTry = scopeTry.ServiceProvider.GetRequiredService<AppDbContext>();
        // quick test query
        await ctxTry.Database.ExecuteSqlRawAsync("SELECT 1");
        selectedConn = candidate;
        Console.WriteLine($"Conectado usando: {candidate}\n");
        break;
    }
    catch (Exception ex)
    {
        lastEx = ex;
        Console.WriteLine($"No se pudo conectar con: {candidate} -> {ex.Message}");
    }
}

if (selectedConn == null)
{
    Console.WriteLine("No se pudo conectar con ninguna cadena probada. Abortando diagnóstico.");
    if (lastEx != null) Console.WriteLine(lastEx);
    return 2;
}
Console.WriteLine("Conectando a BD...\n");

// Build a proper service provider with the selected connection string
var servicesFinal = new ServiceCollection();
servicesFinal.AddDbContext<AppDbContext>(o => o.UseSqlServer(selectedConn!));
using var sp = servicesFinal.BuildServiceProvider();
using var scope = sp.CreateScope();
var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

// Totales generales
var cuposMaximos = await ctx.Talleres.Where(t => t.Estatus == EstatusTaller.Abierto)
    .SumAsync(t => (int?)t.CuposMaximos) ?? 0;
var inscripcionesActivasOpen = await (
    from i in ctx.Inscripciones
    join t in ctx.Talleres on i.TallerId equals t.Id
    where i.Estado == EstadoInscripcion.Activa && t.Estatus == EstatusTaller.Abierto
    select i
).CountAsync();
var inscripcionesActivasTotal = await ctx.Inscripciones.CountAsync(i => i.Estado == EstadoInscripcion.Activa);

Console.WriteLine($"Cupos máximos (talleres abiertos): {cuposMaximos}");
Console.WriteLine($"Inscripciones activas (sobre talleres abiertos): {inscripcionesActivasOpen}");
Console.WriteLine($"Inscripciones activas (total): {inscripcionesActivasTotal}");

var pct = cuposMaximos > 0 ? Math.Round((double)inscripcionesActivasOpen * 100.0 / cuposMaximos, 2) : double.NaN;
Console.WriteLine($"Porcentaje calculado (inscripciones_open / cupos_open): {pct}%\n");

// Per taller breakdown
Console.WriteLine("Desglose por taller (solo Abiertos):");
var perTaller = await (
    from t in ctx.Talleres
    where t.Estatus == EstatusTaller.Abierto
    join i in ctx.Inscripciones.Where(ii => ii.Estado == EstadoInscripcion.Activa) on t.Id equals i.TallerId into g
    select new { t.Id, t.Titulo, t.CuposMaximos, Inscripciones = g.Count() }
).ToListAsync();

Console.WriteLine("Id | CuposMax | InscripcionesAct | %");
foreach (var t in perTaller.OrderByDescending(x=>x.Inscripciones))
{
    var p = t.CuposMaximos > 0 ? Math.Round((double)t.Inscripciones * 100.0 / t.CuposMaximos, 2) : double.NaN;
    Console.WriteLine($"{t.Id} | {t.CuposMaximos} | {t.Inscripciones} | {p}% | {t.Titulo}");
}

// Inscripciones activas que pertenecen a talleres NO abiertos (muestra hasta 20)
var inscNonOpen = await (
    from i in ctx.Inscripciones
    join t in ctx.Talleres on i.TallerId equals t.Id
    where i.Estado == EstadoInscripcion.Activa && t.Estatus != EstatusTaller.Abierto
    select new { i.Id, i.TallerId, TallerEstatus = t.Estatus, t.Titulo }
).Take(20).ToListAsync();

Console.WriteLine($"\nInscripciones activas en talleres NO abiertos (muestra hasta 20): {inscNonOpen.Count}");
foreach (var i in inscNonOpen)
    Console.WriteLine($"Inscripcion {i.Id} -> Taller {i.TallerId} ({i.Titulo}) Estatus={i.TallerEstatus}");

Console.WriteLine("\nDiagnóstico completado.");
return 0;
