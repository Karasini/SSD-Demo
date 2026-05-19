using Transcriptor.Api.Features.TranscriptionJobs.Dtos;

namespace Transcriptor.Api.Features.TranscriptionJobs.Commands;

public record CreateTranscriptionJobCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSizeBytes);

public interface ICreateTranscriptionJobHandler
{
    Task<TranscriptionJobDetailDto> HandleAsync(CreateTranscriptionJobCommand command, CancellationToken cancellationToken = default);
}

public record UpdateTranscriptionJobStatusCommand(
    Guid JobId,
    string Status,
    string? TranscriptText,
    string? DetectedLanguage,
    string? FailureReason);

public interface IUpdateTranscriptionJobStatusHandler
{
    Task<TranscriptionJobDetailDto?> HandleAsync(UpdateTranscriptionJobStatusCommand command, CancellationToken cancellationToken = default);
}

public record TriggerTranscriptionJobCommand(Guid JobId);

public interface ITriggerTranscriptionJobHandler
{
    Task HandleAsync(TriggerTranscriptionJobCommand command, CancellationToken cancellationToken = default);
}
