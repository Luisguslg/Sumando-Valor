namespace SumandoValor.Domain.Entities;

public class CarouselItem
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty; // stored under wwwroot/uploads/carousel/
    public string AltText { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

