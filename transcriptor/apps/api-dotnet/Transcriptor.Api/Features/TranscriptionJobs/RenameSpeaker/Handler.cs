using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.RenameSpeaker.Commands;
using Transcriptor.Api.Features.TranscriptionJobs.RenameSpeaker.Dtos;
using Transcriptor.Api.Infrastructure.DI;

namespace Transcriptor.Api.Features.TranscriptionJobs.RenameSpeaker;

public interface IRenameSpeakerHandler
{
    Task<SpeakerDto?> HandleAsync(
        RenameSpeakerRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class Handler(IRenameSpeakerCommand command) : IRenameSpeakerHandler, IHandler
{
    public Task<SpeakerDto?> HandleAsync(
        RenameSpeakerRequest request,
        CancellationToken cancellationToken = default) =>
        command.ExecuteAsync(request, cancellationToken);
}
