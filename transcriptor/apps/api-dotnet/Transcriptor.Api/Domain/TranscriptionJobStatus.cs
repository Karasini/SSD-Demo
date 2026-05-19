namespace Transcriptor.Api.Domain;

public static class TranscriptionJobStatus
{
    public const string Queued = "Queued";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Failed = "Failed";

    public static bool IsTerminal(string status) =>
        status is Completed or Failed;

    public static bool IsActive(string status) =>
        status is Queued or InProgress;
}
