namespace Infrastructure.Persistence.Configurations;

using Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PdfDocumentConfiguration
    : IEntityTypeConfiguration<PdfDocument>
{
    public void Configure(EntityTypeBuilder<PdfDocument> builder)
    {
        builder.ToTable("pdf_documents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName)
            .IsRequired();

        builder.Property(x => x.FilePath)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>();

        builder.HasIndex(x => x.Status);
    }
}