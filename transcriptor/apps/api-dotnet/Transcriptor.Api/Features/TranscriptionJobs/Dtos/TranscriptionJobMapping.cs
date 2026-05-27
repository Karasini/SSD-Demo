using Transcriptor.Api.Domain;
using System.Text.Json;

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

    public static TranscriptionJobDetailDto ToDetail(this TranscriptionJob job) =>
        CreateDetail(job);

    private static TranscriptionJobDetailDto CreateDetail(TranscriptionJob job)
    {
        var segments = job.Status == TranscriptionJobStatus.Completed
            ? ParseSegments(job.TranscriptSegmentsJson)
            : null;
        var aliases = ParseAliases(job.SpeakerAliasesJson);
        var effectiveSegments = ApplyAliases(segments, aliases);
        var speakers = BuildSpeakers(segments, aliases);

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
            effectiveSegments,
            speakers);
    }

    public static IReadOnlyList<TranscriptSegmentDto>? ParseSegments(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            var segments = JsonSerializer.Deserialize<List<TranscriptSegmentDto>>(json);
            return segments?.Count > 0 ? segments : null;
        }
        catch
        {
            return null;
        }
    }

    public static Dictionary<string, string> ParseAliases(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        try
        {
            var aliases = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return aliases is null
                ? new Dictionary<string, string>(StringComparer.Ordinal)
                : new Dictionary<string, string>(aliases, StringComparer.Ordinal);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }

    public static string SerializeSegments(IReadOnlyList<TranscriptSegmentDto> segments) =>
        JsonSerializer.Serialize(segments);

    public static string SerializeAliases(Dictionary<string, string> aliases) =>
        JsonSerializer.Serialize(aliases);

    private static IReadOnlyList<TranscriptSegmentDto>? ApplyAliases(
        IReadOnlyList<TranscriptSegmentDto>? segments,
        IReadOnlyDictionary<string, string> aliases)
    {
        if (segments is null || segments.Count == 0)
        {
            return null;
        }

        return segments
            .Select(s =>
            {
                var label = aliases.TryGetValue(s.SpeakerId, out var alias) ? alias : s.SpeakerLabel;
                return s with { SpeakerLabel = label };
            })
            .ToList();
    }

    private static IReadOnlyList<SpeakerDto>? BuildSpeakers(
        IReadOnlyList<TranscriptSegmentDto>? segments,
        IReadOnlyDictionary<string, string> aliases)
    {
        if (segments is null || segments.Count == 0)
        {
            return null;
        }

        return segments
            .GroupBy(s => s.SpeakerId)
            .Select(group =>
            {
                var first = group.First();
                var display = aliases.TryGetValue(first.SpeakerId, out var alias) ? alias : first.SpeakerLabel;
                return new SpeakerDto(first.SpeakerId, display, first.SpeakerLabel);
            })
            .OrderBy(s => s.SpeakerId, StringComparer.Ordinal)
            .ToList();
    }
}
