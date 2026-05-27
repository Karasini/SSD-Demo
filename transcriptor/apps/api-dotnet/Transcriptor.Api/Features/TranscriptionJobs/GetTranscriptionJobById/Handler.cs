using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.GetTranscriptionJobById.Dtos;
using Transcriptor.Api.Infrastructure.DI;
using Transcriptor.Api.Infrastructure.Persistence;

namespace Transcriptor.Api.Features.TranscriptionJobs.GetTranscriptionJobById;

public interface IGetTranscriptionJobByIdHandler
{
    Task<TranscriptionJobDetailDto?> HandleAsync(
        GetTranscriptionJobByIdRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class Handler(ITranscriptionJobRepository repository) : IGetTranscriptionJobByIdHandler, IHandler
{
    public async Task<TranscriptionJobDetailDto?> HandleAsync(
        GetTranscriptionJobByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        var job = await repository.GetByIdAsync(request.Id, cancellationToken);
        return job?.ToDetail();
    }
}
