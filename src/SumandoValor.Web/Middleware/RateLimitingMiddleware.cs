using System.Collections.Concurrent;
using System.Net;

namespace SumandoValor.Web.Middleware;

/// <summary>
/// Middleware simple de rate limiting por IP para prevenir abuso en endpoints públicos.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, RateLimitInfo> _requests = new();
    private static readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
    private static DateTime _lastCleanup = DateTime.UtcNow;

    // Límites por endpoint: máximo de requests permitidos en una ventana de tiempo
    private static readonly Dictionary<string, (int MaxRequests, TimeSpan Window)> _limits = new()
    {
        { "/Account/Login", (5, TimeSpan.FromMinutes(15)) },
        { "/Account/Register", (3, TimeSpan.FromHours(1)) },
        { "/Contact", (5, TimeSpan.FromHours(1)) }
    };

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        
        // Solo aplicar rate limiting a endpoints configurados
        if (!_limits.TryGetValue(path, out var limit))
        {
            await _next(context);
            return;
        }

        CleanupOldEntries();

        var clientIp = GetClientIp(context);
        if (string.IsNullOrEmpty(clientIp))
        {
            await _next(context);
            return;
        }

        var key = $"{clientIp}:{path}";
        var now = DateTime.UtcNow;

        var rateLimitInfo = _requests.AddOrUpdate(key,
            new RateLimitInfo { FirstRequest = now, RequestCount = 1 },
            (k, existing) =>
            {
                if (now - existing.FirstRequest > limit.Window)
                {
                    return new RateLimitInfo { FirstRequest = now, RequestCount = 1 };
                }

                existing.RequestCount++;
                return existing;
            });

        if (rateLimitInfo.RequestCount > limit.MaxRequests)
        {
            _logger.LogWarning("Rate limit excedido para {Path} desde IP {Ip}. Requests: {Count}/{Max}",
                path, clientIp, rateLimitInfo.RequestCount, limit.MaxRequests);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync("Demasiadas solicitudes. Por favor intenta más tarde.");
            return;
        }

        await _next(context);
    }

    private static string? GetClientIp(HttpContext context)
    {
        // Considerar X-Forwarded-For para proxies y load balancers
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }

    private static void CleanupOldEntries()
    {
        var now = DateTime.UtcNow;
        if (now - _lastCleanup < _cleanupInterval)
        {
            return;
        }

        _lastCleanup = now;
        var maxAge = _limits.Values.Max(l => l.Window) * 2;

        var keysToRemove = _requests
            .Where(kvp => now - kvp.Value.FirstRequest > maxAge)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _requests.TryRemove(key, out _);
        }
    }

    private class RateLimitInfo
    {
        public DateTime FirstRequest { get; set; }
        public int RequestCount { get; set; }
    }
}
