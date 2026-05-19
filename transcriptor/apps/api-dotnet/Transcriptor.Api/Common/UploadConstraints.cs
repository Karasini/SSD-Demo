namespace Transcriptor.Api.Common;

public static class UploadConstraints
{
    public const long MaxFileSizeBytes = 2_147_483_648L;
    public const int MaxTranscriptTextLength = 10 * 1024 * 1024;

    public static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".wav", ".m4a", ".aac", ".ogg", ".flac",
        ".mp4", ".mov", ".webm", ".mkv", ".avi"
    };

    public static readonly HashSet<string> AllowedContentTypePrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "audio/", "video/"
    };

    public static string AllowedExtensionsDisplay =>
        "mp3, wav, m4a, aac, ogg, flac, mp4, mov, webm, mkv, avi";
}
