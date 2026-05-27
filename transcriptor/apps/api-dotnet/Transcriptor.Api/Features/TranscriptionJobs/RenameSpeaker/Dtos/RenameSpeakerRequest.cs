namespace Transcriptor.Api.Features.TranscriptionJobs.RenameSpeaker.Dtos;

public record RenameSpeakerRequest(
    Guid JobId,
    string SpeakerId,
    string? DisplayName);
