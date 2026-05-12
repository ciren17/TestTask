namespace Application.Interfaces;

public interface ITextExtractor
{
    Task<string> ExtractAsync(
        string filePath,
        CancellationToken cancellationToken);
}