namespace Infrastructure.Services;

using Application.Interfaces;

using UglyToad.PdfPig;

public class PdfTextExtractor : ITextExtractor
{
    public Task<string> ExtractAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        using var document = PdfDocument.Open(filePath);

        var text = string.Join(
            Environment.NewLine,
            document.GetPages().Select(x => x.Text));

        return Task.FromResult(text);
    }
}