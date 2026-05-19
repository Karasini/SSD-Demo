using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.Queries;
using Transcriptor.Api.Infrastructure.Persistence;

namespace Transcriptor.Api.Features.TranscriptionJobs.Handlers;

public class ListTranscriptionJobsHandler(ITranscriptionJobRepository repository) : IListTranscriptionJobsHandler
{
    public async Task<TranscriptionJobListResponseDto> HandleAsync(
        ListTranscriptionJobsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var (items, total) = await repository.ListAsync(page, pageSize, cancellationToken);
        return new TranscriptionJobListResponseDto(
            items.Select(j => j.ToListItem()).ToList(),
            page,
            pageSize,
            total);
    }
}
