using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.ListTranscriptionJobs.Dtos;
using Transcriptor.Api.Infrastructure.DI;
using Transcriptor.Api.Infrastructure.Persistence;

namespace Transcriptor.Api.Features.TranscriptionJobs.ListTranscriptionJobs.Queries;

public interface IListTranscriptionJobsQuery : IQuery
{
    Task<TranscriptionJobListResponseDto> ExecuteAsync(
        ListTranscriptionJobsRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class ListTranscriptionJobsQuery(ITranscriptionJobRepository repository) : IListTranscriptionJobsQuery
{
    public async Task<TranscriptionJobListResponseDto> ExecuteAsync(
        ListTranscriptionJobsRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var (items, total) = await repository.ListAsync(page, pageSize, cancellationToken);
        return new TranscriptionJobListResponseDto(
            items.Select(j => j.ToListItem()).ToList(),
            page,
            pageSize,
            total);
    }
}
