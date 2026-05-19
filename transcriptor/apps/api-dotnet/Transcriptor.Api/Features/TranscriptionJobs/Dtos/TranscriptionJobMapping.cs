using Transcriptor.Api.Domain;

namespace Transcriptor.Api.Features.TranscriptionJobs.Dtos;

public static class TranscriptionJobMapping
{
    public static TranscriptionJobListItemDto ToListItem(this TranscriptionJob job) =>
        new(
            job.Id,
            job.FileName,
            job.FileSizeBytes,
            job.ContentType,
            job.Status,
            job.CreatedAt,
            job.UpdatedAt,
            job.CompletedAt,
            job.FailureReason);

    public static TranscriptionJobDetailDto ToDetail(this TranscriptionJob job) =>
        new(
            job.Id,
            job.FileName,
            job.FileSizeBytes,
            job.ContentType,
            job.Status,
            job.CreatedAt,
            job.UpdatedAt,
            job.CompletedAt,
            job.FailureReason,
            job.Status == TranscriptionJobStatus.Completed ? job.TranscriptText : null,
            job.DetectedLanguage);
}
