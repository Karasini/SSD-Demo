namespace Transcriptor.Api.Features.TranscriptionJobs.Dtos;

public record TranscriptionJobListItemDto(
    Guid Id,
    string FileName,
    long FileSizeBytes,
    string ContentType,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? CompletedAt,
    string? FailureReason);

public record TranscriptionJobDetailDto(
    Guid Id,
    string FileName,
    long FileSizeBytes,
    string ContentType,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? CompletedAt,
    string? FailureReason,
    string? TranscriptText,
    string? DetectedLanguage,
    bool HasDiarization,
    IReadOnlyList<TranscriptSegmentDto> Segments,
    IReadOnlyList<TranscriptionSpeakerDto> Speakers) : TranscriptionJobListItemDto(
    Id, FileName, FileSizeBytes, ContentType, Status,
    CreatedAt, UpdatedAt, CompletedAt, FailureReason);

public record TranscriptionJobListResponseDto(
    IReadOnlyList<TranscriptionJobListItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public record UpdateTranscriptionJobRequestDto(
    string Status,
    string? TranscriptText,
    string? DetectedLanguage,
    string? FailureReason,
    bool? HasDiarization,
    IReadOnlyList<CallbackTranscriptSegmentDto>? Segments,
    IReadOnlyList<CallbackTranscriptionSpeakerDto>? Speakers);

public record BulkDeleteTranscriptionJobsRequestDto(IReadOnlyList<Guid> Ids);

public record BulkDeleteFailureItemDto(Guid Id, string Reason);

public record BulkDeleteTranscriptionJobsResponseDto(
    IReadOnlyList<Guid> DeletedIds,
    IReadOnlyList<BulkDeleteFailureItemDto> Failed);
