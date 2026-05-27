namespace Transcriptor.Api.Domain;

public class TranscriptionJob
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Status { get; set; } = TranscriptionJobStatus.Queued;
    public string StorageKey { get; set; } = string.Empty;
    public string? TranscriptText { get; set; }
    public string? TranscriptSegmentsJson { get; set; }
    public string? SpeakerLabelsJson { get; set; }
    public string? DetectedLanguage { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
