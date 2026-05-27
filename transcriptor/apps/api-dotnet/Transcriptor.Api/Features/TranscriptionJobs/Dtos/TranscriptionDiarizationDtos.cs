namespace Transcriptor.Api.Features.TranscriptionJobs.Dtos;

public record TranscriptSegmentDto(
    int Index,
    string SpeakerKey,
    double StartSeconds,
    double EndSeconds,
    string Text);

public record TranscriptionSpeakerDto(
    string SpeakerKey,
    string DefaultLabel,
    string DisplayName);

public record CallbackTranscriptSegmentDto(
    int Index,
    string SpeakerKey,
    double StartSeconds,
    double EndSeconds,
    string Text);

public record CallbackTranscriptionSpeakerDto(
    string SpeakerKey,
    string DefaultLabel);

public record UpdateTranscriptionSpeakerLabelRequestDto(string DisplayName);
