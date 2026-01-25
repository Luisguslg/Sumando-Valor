namespace SumandoValor.Domain.Entities.Surveys;

public class SurveyTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Scope: either CursoId or TallerId (one of them must be set)
    public int? CursoId { get; set; }
    public int? TallerId { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SurveyQuestion> Questions { get; set; } = new List<SurveyQuestion>();
}

