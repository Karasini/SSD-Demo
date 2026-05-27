using Transcriptor.Api.Common;
using Transcriptor.Api.Domain;
using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.Exceptions;
using Transcriptor.Api.Features.TranscriptionJobs.UpdateTranscriptionJobStatus.Dtos;
using Transcriptor.Api.Infrastructure.DI;
using Transcriptor.Api.Infrastructure.Persistence;

namespace Transcriptor.Api.Features.TranscriptionJobs.UpdateTranscriptionJobStatus.Commands;

public interface IUpdateTranscriptionJobStatusCommand : ICommand
{
    Task<TranscriptionJobDetailDto?> ExecuteAsync(
        UpdateTranscriptionJobStatusRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateTranscriptionJobStatusCommand(ITranscriptionJobRepository repository)
    : IUpdateTranscriptionJobStatusCommand
{
    public async Task<TranscriptionJobDetailDto?> ExecuteAsync(
        UpdateTranscriptionJobStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var job = await repository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null)
        {
            return null;
        }

        if (TranscriptionJobStatus.IsTerminal(job.Status))
        {
            return job.ToDetail();
        }

        var now = DateTimeOffset.UtcNow;
        job.UpdatedAt = now;

        switch (request.Status)
        {
            case TranscriptionJobStatus.InProgress:
                if (job.Status == TranscriptionJobStatus.Queued)
                {
                    job.Status = TranscriptionJobStatus.InProgress;
                }
                break;

            case TranscriptionJobStatus.Completed:
                if (request.TranscriptText is null)
                {
                    throw new ValidationException("Transcript text is required when status is Completed.");
                }

                if (request.TranscriptText.Length > UploadConstraints.MaxTranscriptTextLength)
                {
                    job.Status = TranscriptionJobStatus.Failed;
                    job.FailureReason = "Transcript is too large to store.";
                }
                else
                {
                    var segments = BuildSegments(request);
                    job.Status = TranscriptionJobStatus.Completed;
                    job.TranscriptText = request.TranscriptText;
                    job.TranscriptSegmentsJson = TranscriptionJobMapping.SerializeSegments(segments);
                    job.DetectedLanguage = request.DetectedLanguage;
                    job.FailureReason = null;
                    job.CompletedAt = now;
                }
                break;

            case TranscriptionJobStatus.Failed:
                job.Status = TranscriptionJobStatus.Failed;
                job.FailureReason = request.FailureReason
                    ?? "Transcription failed. Please try again with another file.";
                break;

            default:
                throw new ValidationException($"Unsupported status transition to {request.Status}.");
        }

        await repository.SaveChangesAsync(cancellationToken);
        return job.ToDetail();
    }

    private static IReadOnlyList<TranscriptSegmentDto> BuildSegments(UpdateTranscriptionJobStatusRequest request)
    {
        if (request.Segments is null || request.Segments.Count == 0)
        {
            return
            [
                new TranscriptSegmentDto("spk_1", "Speaker", 0, 0.01, request.TranscriptText ?? string.Empty)
            ];
        }

        var result = new List<TranscriptSegmentDto>(request.Segments.Count);
        foreach (var segment in request.Segments)
        {
            if (string.IsNullOrWhiteSpace(segment.SpeakerId))
            {
                throw new ValidationException("Segment speakerId is required.");
            }

            if (string.IsNullOrWhiteSpace(segment.SpeakerLabel))
            {
                throw new ValidationException("Segment speakerLabel is required.");
            }

            if (string.IsNullOrWhiteSpace(segment.Text))
            {
                throw new ValidationException("Segment text is required.");
            }

            if (segment.StartSec < 0 || segment.EndSec <= segment.StartSec)
            {
                throw new ValidationException("Segment start/end time is invalid.");
            }

            result.Add(segment);
        }

        return result;
    }
}
