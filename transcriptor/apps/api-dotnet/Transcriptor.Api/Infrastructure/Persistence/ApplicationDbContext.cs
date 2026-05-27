using Microsoft.EntityFrameworkCore;
using Transcriptor.Api.Domain;

namespace Transcriptor.Api.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<TranscriptionJob> TranscriptionJobs => Set<TranscriptionJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TranscriptionJob>(entity =>
        {
            entity.ToTable("TranscriptionJobs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(512).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(128).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(32).IsRequired();
            entity.Property(e => e.StorageKey).HasMaxLength(1024).IsRequired();
            entity.Property(e => e.DetectedLanguage).HasMaxLength(16);
            entity.Property(e => e.FailureReason).HasMaxLength(2000);
            entity.Property(e => e.TranscriptText).HasColumnType("text");
            entity.Property(e => e.TranscriptSegmentsJson).HasColumnType("jsonb");
            entity.Property(e => e.SpeakerLabelsJson).HasColumnType("jsonb");
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
