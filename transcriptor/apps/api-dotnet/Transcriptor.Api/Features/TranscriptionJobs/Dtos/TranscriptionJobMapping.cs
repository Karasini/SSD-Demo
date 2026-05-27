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

    public static TranscriptionJobDetailDto ToDetail(this TranscriptionJob job)
    {
        var segments = TranscriptionJobDiarization.ParseSegments(job.TranscriptSegmentsJson);
        var hasDiarization = segments.Count > 0;
        var labelOverrides = TranscriptionJobDiarization.ParseSpeakerLabels(job.SpeakerLabelsJson);
        var segmentDtos = hasDiarization
            ? TranscriptionJobDiarization.ToSegmentDtos(segments)
            : Array.Empty<TranscriptSegmentDto>();
        var speakers = hasDiarization
            ? TranscriptionJobDiarization.BuildSpeakers(segments, labelOverrides)
            : Array.Empty<TranscriptionSpeakerDto>();

        return new TranscriptionJobDetailDto(
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
            job.DetectedLanguage,
            hasDiarization,
            segmentDtos,
            speakers);
    }
}
