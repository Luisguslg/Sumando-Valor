using SumandoValor.Domain.Entities;

namespace SumandoValor.Web.Pages.Shared;

public sealed class WorkshopCardModel
{
    public int TallerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }

    public DateTime FechaInicio { get; init; }
    public TimeSpan HoraInicio { get; init; }
    public ModalidadTaller Modalidad { get; init; }

    public int CuposDisponibles { get; init; }
    public int CuposMaximos { get; init; }
    public EstatusTaller Estatus { get; init; }

    public string? CursoTitle { get; init; }
    public string PrimaryCtaText { get; init; } = "Ver evento";
    public string PrimaryCtaPage { get; init; } = "/Talleres/Details";
}

