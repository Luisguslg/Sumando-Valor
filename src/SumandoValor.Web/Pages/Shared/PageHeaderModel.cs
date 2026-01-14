namespace SumandoValor.Web.Pages.Shared;

public sealed class PageHeaderModel
{
    public string Title { get; init; } = string.Empty;
    public string? Subtitle { get; init; }

    /// <summary>
    /// URL de imagen de fondo (opcional). Si se especifica, se usa estilo hero.
    /// </summary>
    public string? BackgroundImageUrl { get; init; }

    /// <summary>
    /// Si es true, aplica el fondo de marca (cta-section) aun sin imagen.
    /// </summary>
    public bool UseBrandBackground { get; init; }
}

