namespace SumandoValor.Domain.Entities.Surveys;

public class SurveyQuestion
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public SurveyQuestionType Type { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    // Options/config, depending on Type:
    // - SingleChoice: JSON array of string options
    // - ScoreNumber:  JSON object { "min": 1, "max": 10, "step": 1 }
    public string OptionsJson { get; set; } = string.Empty;

    public SurveyTemplate Template { get; set; } = null!;
}

public enum SurveyQuestionType
{
    Rating1To5 = 1,   // radio 1..5
    Text = 2,         // short/medium text
    SingleChoice = 3, // radio options
    ScoreNumber = 4,  // numeric score (min/max/step in OptionsJson)
    Description = 5   // long text/description
}

