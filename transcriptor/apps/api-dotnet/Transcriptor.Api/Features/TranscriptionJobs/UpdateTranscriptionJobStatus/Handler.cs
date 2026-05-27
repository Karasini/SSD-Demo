using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.UpdateTranscriptionJobStatus.Commands;
using Transcriptor.Api.Features.TranscriptionJobs.UpdateTranscriptionJobStatus.Dtos;
using Transcriptor.Api.Infrastructure.DI;

namespace Transcriptor.Api.Features.TranscriptionJobs.UpdateTranscriptionJobStatus;

public interface IUpdateTranscriptionJobStatusHandler
{
    Task<TranscriptionJobDetailDto?> HandleAsync(
        UpdateTranscriptionJobStatusRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class Handler(IUpdateTranscriptionJobStatusCommand command) : IUpdateTranscriptionJobStatusHandler, IHandler
{
    public Task<TranscriptionJobDetailDto?> HandleAsync(
        UpdateTranscriptionJobStatusRequest request,
        CancellationToken cancellationToken = default) =>
        command.ExecuteAsync(request, cancellationToken);
}
