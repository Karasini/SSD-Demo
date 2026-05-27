using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.UpdateTranscriptionSpeakerLabel.Commands;
using Transcriptor.Api.Features.TranscriptionJobs.UpdateTranscriptionSpeakerLabel.Dtos;
using Transcriptor.Api.Infrastructure.DI;

namespace Transcriptor.Api.Features.TranscriptionJobs.UpdateTranscriptionSpeakerLabel;

public interface IUpdateTranscriptionSpeakerLabelHandler
{
    Task<TranscriptionJobDetailDto?> HandleAsync(
        UpdateTranscriptionSpeakerLabelRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class Handler(IUpdateTranscriptionSpeakerLabelCommand command)
    : IUpdateTranscriptionSpeakerLabelHandler, IHandler
{
    public Task<TranscriptionJobDetailDto?> HandleAsync(
        UpdateTranscriptionSpeakerLabelRequest request,
        CancellationToken cancellationToken = default) =>
        command.ExecuteAsync(request, cancellationToken);
}
