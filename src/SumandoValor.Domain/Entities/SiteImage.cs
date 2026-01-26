namespace SumandoValor.Domain.Entities;

/// <summary>
/// Simple storage for admin-managed images used across the site (non-carousel).
/// Files are stored under wwwroot/uploads/site/.
/// </summary>
public class SiteImage
{
    public int Id { get; set; }

    /// <summary>
    /// Logical slot key (e.g. "AboutMain", "WorkshopCard").
    /// </summary>
    public string Key { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

