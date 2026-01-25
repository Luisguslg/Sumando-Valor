namespace SumandoValor.Domain.Entities.Surveys;

public class SurveyQuestion
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public SurveyQuestionType Type { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    public string OptionsJson { get; set; } = string.Empty; // for SingleChoice: JSON array of options

    public SurveyTemplate Template { get; set; } = null!;
}

public enum SurveyQuestionType
{
    Rating1To5 = 1,
    Text = 2,
    SingleChoice = 3
}

