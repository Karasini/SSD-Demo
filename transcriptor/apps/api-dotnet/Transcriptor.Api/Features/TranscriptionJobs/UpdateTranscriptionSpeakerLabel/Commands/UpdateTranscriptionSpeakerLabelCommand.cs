using Transcriptor.Api.Domain;
using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.Exceptions;
using Transcriptor.Api.Features.TranscriptionJobs.UpdateTranscriptionSpeakerLabel.Dtos;
using Transcriptor.Api.Infrastructure.DI;
using Transcriptor.Api.Infrastructure.Persistence;

namespace Transcriptor.Api.Features.TranscriptionJobs.UpdateTranscriptionSpeakerLabel.Commands;

public interface IUpdateTranscriptionSpeakerLabelCommand : ICommand
{
    Task<TranscriptionJobDetailDto?> ExecuteAsync(
        UpdateTranscriptionSpeakerLabelRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateTranscriptionSpeakerLabelCommand(ITranscriptionJobRepository repository)
    : IUpdateTranscriptionSpeakerLabelCommand
{
    public async Task<TranscriptionJobDetailDto?> ExecuteAsync(
        UpdateTranscriptionSpeakerLabelRequest request,
        CancellationToken cancellationToken = default)
    {
        var job = await repository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null)
        {
            return null;
        }

        if (job.Status != TranscriptionJobStatus.Completed)
        {
            throw new ConflictException("Speaker labels can only be changed on completed jobs.");
        }

        var segments = TranscriptionJobDiarization.ParseSegments(job.TranscriptSegmentsJson);
        if (segments.Count == 0)
        {
            throw new ConflictException("Speaker rename is not available for this job.");
        }

        if (!segments.Any(s => string.Equals(s.SpeakerKey, request.SpeakerKey, StringComparison.Ordinal)))
        {
            throw new ResourceNotFoundException("Speaker was not found for this job.");
        }

        var displayName = TranscriptionJobDiarization.ValidateDisplayName(request.DisplayName);
        var labels = TranscriptionJobDiarization.ParseSpeakerLabels(job.SpeakerLabelsJson);
        labels[request.SpeakerKey] = displayName;
        job.SpeakerLabelsJson = TranscriptionJobDiarization.SerializeSpeakerLabels(labels);
        job.UpdatedAt = DateTimeOffset.UtcNow;

        await repository.SaveChangesAsync(cancellationToken);
        return job.ToDetail();
    }
}
