namespace Transcriptor.Api.Features.TranscriptionJobs.Exceptions;

public sealed class ValidationException(string message) : Exception(message);
