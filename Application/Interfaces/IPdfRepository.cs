namespace Application.Interfaces;

using Domain.Entities;

public interface IPdfRepository
{
    Task AddAsync(PdfDocument document, CancellationToken cancellationToken);

    Task<List<PdfDocument>> GetAllAsync(CancellationToken cancellationToken);

    Task<PdfDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}