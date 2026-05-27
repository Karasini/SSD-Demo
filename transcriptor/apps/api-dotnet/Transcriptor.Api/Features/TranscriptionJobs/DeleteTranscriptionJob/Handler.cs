using Transcriptor.Api.Features.TranscriptionJobs.DeleteTranscriptionJob.Commands;
using Transcriptor.Api.Features.TranscriptionJobs.DeleteTranscriptionJob.Dtos;
using Transcriptor.Api.Infrastructure.DI;

namespace Transcriptor.Api.Features.TranscriptionJobs.DeleteTranscriptionJob;

public interface IDeleteTranscriptionJobHandler
{
    /// <returns>True if deleted; false if job was not found.</returns>
    Task<bool> HandleAsync(DeleteTranscriptionJobRequest request, CancellationToken cancellationToken = default);
}

public sealed class Handler(IDeleteTranscriptionJobCommand command) : IDeleteTranscriptionJobHandler, IHandler
{
    public Task<bool> HandleAsync(
        DeleteTranscriptionJobRequest request,
        CancellationToken cancellationToken = default) =>
        command.ExecuteAsync(request, cancellationToken);
}
