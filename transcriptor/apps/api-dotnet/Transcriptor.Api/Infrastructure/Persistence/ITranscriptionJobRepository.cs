using Transcriptor.Api.Domain;

namespace Transcriptor.Api.Infrastructure.Persistence;

public interface ITranscriptionJobRepository
{
    Task AddAsync(TranscriptionJob job, CancellationToken cancellationToken = default);
    Task<TranscriptionJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<TranscriptionJob> Items, int TotalCount)> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task RemoveAsync(TranscriptionJob job, CancellationToken cancellationToken = default);
}
