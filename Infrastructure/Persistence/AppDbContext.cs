namespace Infrastructure.Persistence;

using Domain.Entities;

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<PdfDocument> PdfDocuments => Set<PdfDocument>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}