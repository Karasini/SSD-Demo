namespace Transcriptor.Api.Features.TranscriptionJobs.CreateTranscriptionJob.Exceptions;

public sealed class PayloadTooLargeException(string message) : Exception(message);
