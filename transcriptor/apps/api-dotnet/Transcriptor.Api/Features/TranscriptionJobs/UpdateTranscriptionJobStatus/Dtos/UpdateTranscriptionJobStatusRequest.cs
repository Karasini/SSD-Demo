using Transcriptor.Api.Features.TranscriptionJobs.Dtos;

namespace Transcriptor.Api.Features.TranscriptionJobs.UpdateTranscriptionJobStatus.Dtos;

public record UpdateTranscriptionJobStatusRequest(
    Guid JobId,
    string Status,
    string? TranscriptText,
    string? DetectedLanguage,
    string? FailureReason,
    bool? HasDiarization,
    IReadOnlyList<CallbackTranscriptSegmentDto>? Segments,
    IReadOnlyList<CallbackTranscriptionSpeakerDto>? Speakers);
