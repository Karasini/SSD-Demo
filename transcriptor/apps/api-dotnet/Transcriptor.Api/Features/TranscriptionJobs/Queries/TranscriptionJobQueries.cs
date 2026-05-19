using Transcriptor.Api.Features.TranscriptionJobs.Dtos;

namespace Transcriptor.Api.Features.TranscriptionJobs.Queries;

public record ListTranscriptionJobsQuery(int Page, int PageSize);

public interface IListTranscriptionJobsHandler
{
    Task<TranscriptionJobListResponseDto> HandleAsync(ListTranscriptionJobsQuery query, CancellationToken cancellationToken = default);
}

public record GetTranscriptionJobByIdQuery(Guid Id);

public interface IGetTranscriptionJobByIdHandler
{
    Task<TranscriptionJobDetailDto?> HandleAsync(GetTranscriptionJobByIdQuery query, CancellationToken cancellationToken = default);
}
