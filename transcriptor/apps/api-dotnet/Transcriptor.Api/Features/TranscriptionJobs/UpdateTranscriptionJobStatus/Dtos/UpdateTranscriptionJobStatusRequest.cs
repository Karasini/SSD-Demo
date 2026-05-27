namespace Transcriptor.Api.Features.TranscriptionJobs.UpdateTranscriptionJobStatus.Dtos;

public record UpdateTranscriptionJobStatusRequest(
    Guid JobId,
    string Status,
    string? TranscriptText,
    string? DetectedLanguage,
    string? FailureReason);
