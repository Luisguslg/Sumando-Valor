namespace SumandoValor.Domain.Entities.Surveys;

public class SurveyResponse
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public int TallerId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SurveyTemplate Template { get; set; } = null!;
    public ICollection<SurveyAnswer> Answers { get; set; } = new List<SurveyAnswer>();
}

