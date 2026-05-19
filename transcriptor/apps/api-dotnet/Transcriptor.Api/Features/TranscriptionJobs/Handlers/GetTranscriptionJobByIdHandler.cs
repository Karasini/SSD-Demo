using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.Queries;
using Transcriptor.Api.Infrastructure.Persistence;

namespace Transcriptor.Api.Features.TranscriptionJobs.Handlers;

public class GetTranscriptionJobByIdHandler(ITranscriptionJobRepository repository) : IGetTranscriptionJobByIdHandler
{
    public async Task<TranscriptionJobDetailDto?> HandleAsync(
        GetTranscriptionJobByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var job = await repository.GetByIdAsync(query.Id, cancellationToken);
        return job?.ToDetail();
    }
}
