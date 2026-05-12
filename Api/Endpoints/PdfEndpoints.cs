namespace Api.Endpoints;

using Application.Interfaces;
using Application.Messages;
using Domain.Entities;
using Domain.Enums;

using Microsoft.AspNetCore.Mvc;

public static class PdfEndpoints
{
    public static IEndpointRouteBuilder MapPdfEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pdf");

        group.MapPost("/upload", UploadAsync).DisableAntiforgery();
        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:guid}/text", GetTextAsync);

        return app;
    }

    private static async Task<IResult> UploadAsync(
        IFormFile file,
        [FromServices] IPdfRepository repository,
        [FromServices] IMessagePublisher publisher,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return Results.BadRequest();

        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "storage");

        Directory.CreateDirectory(uploads);

        var id = Guid.NewGuid();

        var path = Path.Combine(
            uploads,
            $"{id}.pdf");

        await using (var stream = File.Create(path))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var document = new PdfDocument
        {
            Id = id,
            FileName = file.FileName,
            FilePath = path,
            Status = DocumentStatus.Uploaded,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(document, cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        await publisher.PublishAsync(
            new PdfUploadedMessage
            {
                DocumentId = id
            },
            cancellationToken);

        return Results.Ok(document.Id);
    }

    private static async Task<IResult> GetAllAsync(
        [FromServices] IPdfRepository repository,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetAllAsync(cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetTextAsync(
        Guid id,
        [FromServices] IPdfRepository repository,
        CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(
            id,
            cancellationToken);

        if (document is null)
            return Results.NotFound();

        return Results.Ok(document.ExtractedText);
    }
}