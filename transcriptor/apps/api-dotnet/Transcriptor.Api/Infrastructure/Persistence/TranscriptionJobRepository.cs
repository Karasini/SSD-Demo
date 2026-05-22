using Microsoft.EntityFrameworkCore;
using Transcriptor.Api.Domain;

namespace Transcriptor.Api.Infrastructure.Persistence;

public class TranscriptionJobRepository(ApplicationDbContext db) : ITranscriptionJobRepository
{
    public async Task AddAsync(TranscriptionJob job, CancellationToken cancellationToken = default) =>
        await db.TranscriptionJobs.AddAsync(job, cancellationToken);

    public Task<TranscriptionJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.TranscriptionJobs.FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<TranscriptionJob> Items, int TotalCount)> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = db.TranscriptionJobs.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);

    public Task RemoveAsync(TranscriptionJob job, CancellationToken cancellationToken = default)
    {
        db.TranscriptionJobs.Remove(job);
        return Task.CompletedTask;
    }
}
