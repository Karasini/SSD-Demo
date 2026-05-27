namespace Transcriptor.Api.Domain;

public sealed class TranscriptSegment
{
    public int Index { get; set; }
    public string SpeakerKey { get; set; } = string.Empty;
    public double StartSeconds { get; set; }
    public double EndSeconds { get; set; }
    public string Text { get; set; } = string.Empty;
}
