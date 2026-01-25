namespace SumandoValor.Domain.Entities.Surveys;

public class SurveyAnswer
{
    public int Id { get; set; }
    public int ResponseId { get; set; }
    public int QuestionId { get; set; }

    // store as string (works for rating/text/single-choice)
    public string Value { get; set; } = string.Empty;

    public SurveyResponse Response { get; set; } = null!;
}

