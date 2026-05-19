using Transcriptor.Api.Common;
using Transcriptor.Api.Domain;
using Transcriptor.Api.Features.TranscriptionJobs.Commands;
using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Infrastructure.Persistence;

namespace Transcriptor.Api.Features.TranscriptionJobs.Handlers;

public class UpdateTranscriptionJobStatusHandler(ITranscriptionJobRepository repository)
    : IUpdateTranscriptionJobStatusHandler
{
    public async Task<TranscriptionJobDetailDto?> HandleAsync(
        UpdateTranscriptionJobStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        var job = await repository.GetByIdAsync(command.JobId, cancellationToken);
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

        switch (command.Status)
        {
            case TranscriptionJobStatus.InProgress:
                if (job.Status == TranscriptionJobStatus.Queued)
                {
                    job.Status = TranscriptionJobStatus.InProgress;
                }
                break;

            case TranscriptionJobStatus.Completed:
                if (command.TranscriptText is null)
                {
                    throw new ValidationException("Transcript text is required when status is Completed.");
                }

                if (command.TranscriptText.Length > UploadConstraints.MaxTranscriptTextLength)
                {
                    job.Status = TranscriptionJobStatus.Failed;
                    job.FailureReason = "Transcript is too large to store.";
                }
                else
                {
                    job.Status = TranscriptionJobStatus.Completed;
                    job.TranscriptText = command.TranscriptText;
                    job.DetectedLanguage = command.DetectedLanguage;
                    job.CompletedAt = now;
                }
                break;

            case TranscriptionJobStatus.Failed:
                job.Status = TranscriptionJobStatus.Failed;
                job.FailureReason = command.FailureReason
                    ?? "Transcription failed. Please try again with another file.";
                break;

            default:
                throw new ValidationException($"Unsupported status transition to {command.Status}.");
        }

        await repository.SaveChangesAsync(cancellationToken);
        return job.ToDetail();
    }
}
