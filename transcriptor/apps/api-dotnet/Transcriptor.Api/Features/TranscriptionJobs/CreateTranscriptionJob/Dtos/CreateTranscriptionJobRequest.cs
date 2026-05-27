namespace Transcriptor.Api.Features.TranscriptionJobs.CreateTranscriptionJob.Dtos;

public record CreateTranscriptionJobRequest(
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSizeBytes);
