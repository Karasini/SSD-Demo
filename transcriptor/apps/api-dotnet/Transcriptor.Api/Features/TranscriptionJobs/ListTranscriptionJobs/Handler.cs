using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.ListTranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.ListTranscriptionJobs.Queries;
using Transcriptor.Api.Infrastructure.DI;

namespace Transcriptor.Api.Features.TranscriptionJobs.ListTranscriptionJobs;

public interface IListTranscriptionJobsHandler
{
    Task<TranscriptionJobListResponseDto> HandleAsync(
        ListTranscriptionJobsRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class Handler(IListTranscriptionJobsQuery query) : IListTranscriptionJobsHandler, IHandler
{
    public Task<TranscriptionJobListResponseDto> HandleAsync(
        ListTranscriptionJobsRequest request,
        CancellationToken cancellationToken = default) =>
        query.ExecuteAsync(request, cancellationToken);
}
