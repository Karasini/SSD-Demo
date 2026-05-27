using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.Exceptions;
using Transcriptor.Api.Features.TranscriptionJobs.RenameSpeaker.Dtos;
using Transcriptor.Api.Infrastructure.DI;
using Transcriptor.Api.Infrastructure.Persistence;

namespace Transcriptor.Api.Features.TranscriptionJobs.RenameSpeaker.Commands;

public interface IRenameSpeakerCommand : ICommand
{
    Task<SpeakerDto?> ExecuteAsync(
        RenameSpeakerRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class RenameSpeakerCommand(ITranscriptionJobRepository repository) : IRenameSpeakerCommand
{
    public async Task<SpeakerDto?> ExecuteAsync(
        RenameSpeakerRequest request,
        CancellationToken cancellationToken = default)
    {
        var displayName = request.DisplayName?.Trim();
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ValidationException("Display name is required.");
        }

        if (displayName.Length > 80)
        {
            throw new ValidationException("Display name is too long.");
        }

        var job = await repository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null)
        {
            return null;
        }

        var segments = TranscriptionJobMapping.ParseSegments(job.TranscriptSegmentsJson);
        if (segments is null || segments.Count == 0)
        {
            return null;
        }

        var speaker = segments.FirstOrDefault(s => s.SpeakerId == request.SpeakerId);
        if (speaker is null)
        {
            return null;
        }

        var aliases = TranscriptionJobMapping.ParseAliases(job.SpeakerAliasesJson);
        aliases[request.SpeakerId] = displayName;
        job.SpeakerAliasesJson = TranscriptionJobMapping.SerializeAliases(aliases);
        job.UpdatedAt = DateTimeOffset.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);
        return new SpeakerDto(request.SpeakerId, displayName, speaker.SpeakerLabel);
    }
}
