namespace Domain.Entities;

using Domain.Enums;

public class PdfDocument
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public string? ExtractedText { get; set; }

    public DocumentStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }
}