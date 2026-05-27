using System.Text.Json;
using System.Text.Json.Serialization;
using Transcriptor.Api.Domain;

namespace Transcriptor.Api.Features.TranscriptionJobs.Dtos;

public static class TranscriptionJobDiarization
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public const int MaxDisplayNameLength = 64;

    public static IReadOnlyList<TranscriptSegment> ParseSegments(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<TranscriptSegment>();
        }

        return JsonSerializer.Deserialize<List<TranscriptSegment>>(json, JsonOptions)
            ?? new List<TranscriptSegment>();
    }

    public static string SerializeSegments(IReadOnlyList<TranscriptSegment> segments) =>
        JsonSerializer.Serialize(segments, JsonOptions);

    public static Dictionary<string, string> ParseSpeakerLabels(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        return JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions)
            ?? new Dictionary<string, string>(StringComparer.Ordinal);
    }

    public static string SerializeSpeakerLabels(Dictionary<string, string> labels) =>
        JsonSerializer.Serialize(labels, JsonOptions);

    public static bool HasDiarizationData(TranscriptionJob job) =>
        ParseSegments(job.TranscriptSegmentsJson).Count > 0;

    public static IReadOnlyList<TranscriptionSpeakerDto> BuildSpeakers(
        IReadOnlyList<TranscriptSegment> segments,
        Dictionary<string, string> labelOverrides)
    {
        var keysInOrder = new List<string>();
        foreach (var segment in segments.OrderBy(s => s.Index))
        {
            if (!keysInOrder.Contains(segment.SpeakerKey, StringComparer.Ordinal))
            {
                keysInOrder.Add(segment.SpeakerKey);
            }
        }

        return keysInOrder
            .Select((key, index) =>
            {
                var defaultLabel = $"Person {index + 1}";
                var displayName = labelOverrides.TryGetValue(key, out var overrideName)
                    ? overrideName
                    : defaultLabel;
                return new TranscriptionSpeakerDto(key, defaultLabel, displayName);
            })
            .ToList();
    }

    public static IReadOnlyList<TranscriptSegmentDto> ToSegmentDtos(IReadOnlyList<TranscriptSegment> segments) =>
        segments
            .OrderBy(s => s.Index)
            .Select(s => new TranscriptSegmentDto(
                s.Index,
                s.SpeakerKey,
                s.StartSeconds,
                s.EndSeconds,
                s.Text))
            .ToList();

    public static void ValidateCompletedCallback(
        string? transcriptText,
        bool? hasDiarization,
        IReadOnlyList<CallbackTranscriptSegmentDto>? segments,
        IReadOnlyList<CallbackTranscriptionSpeakerDto>? speakers)
    {
        if (transcriptText is null)
        {
            throw new Exceptions.ValidationException("Transcript text is required when status is Completed.");
        }

        if (hasDiarization != true)
        {
            throw new Exceptions.ValidationException(
                "Has diarization must be true when status is Completed.");
        }

        if (segments is null || segments.Count == 0)
        {
            throw new Exceptions.ValidationException(
                "Segments are required when status is Completed.");
        }

        var segmentSpeakerKeys = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            if (string.IsNullOrWhiteSpace(segment.SpeakerKey))
            {
                throw new Exceptions.ValidationException($"Segment {i} requires a speaker key.");
            }

            if (string.IsNullOrWhiteSpace(segment.Text))
            {
                throw new Exceptions.ValidationException($"Segment {i} requires text.");
            }

            if (segment.EndSeconds < segment.StartSeconds)
            {
                throw new Exceptions.ValidationException(
                    $"Segment {i} end time must be greater than or equal to start time.");
            }

            segmentSpeakerKeys.Add(segment.SpeakerKey);
        }

        if (speakers is null || speakers.Count == 0)
        {
            throw new Exceptions.ValidationException(
                "Speakers are required when status is Completed.");
        }

        var declaredKeys = speakers.Select(s => s.SpeakerKey).ToHashSet(StringComparer.Ordinal);
        foreach (var key in segmentSpeakerKeys)
        {
            if (!declaredKeys.Contains(key))
            {
                throw new Exceptions.ValidationException(
                    $"Speaker catalog must include key {key} used in segments.");
            }
        }
    }

    public static string ValidateDisplayName(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new Exceptions.ValidationException("Display name is required.");
        }

        var trimmed = displayName.Trim();
        if (trimmed.Length > MaxDisplayNameLength)
        {
            throw new Exceptions.ValidationException(
                $"Display name must be at most {MaxDisplayNameLength} characters.");
        }

        return trimmed;
    }
}
