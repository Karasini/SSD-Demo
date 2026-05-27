namespace Transcriptor.Api.Features.TranscriptionJobs.UpdateTranscriptionSpeakerLabel.Dtos;

public record UpdateTranscriptionSpeakerLabelRequest(
    Guid JobId,
    string SpeakerKey,
    string DisplayName);
