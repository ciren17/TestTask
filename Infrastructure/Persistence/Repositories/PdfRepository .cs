namespace Infrastructure.Repositories;

using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class PdfRepository : IPdfRepository
{
    private readonly AppDbContext _context;

    public PdfRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        PdfDocument document,
        CancellationToken cancellationToken)
    {
        await _context.PdfDocuments.AddAsync(
            document,
            cancellationToken);
    }

    public Task<List<PdfDocument>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return _context.PdfDocuments.ToListAsync(cancellationToken);
    }

    public Task<PdfDocument?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return _context.PdfDocuments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}