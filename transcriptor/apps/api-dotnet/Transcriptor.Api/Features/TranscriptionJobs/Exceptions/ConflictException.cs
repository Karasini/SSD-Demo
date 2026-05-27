namespace Transcriptor.Api.Features.TranscriptionJobs.Exceptions;

public sealed class ConflictException(string message) : Exception(message);
