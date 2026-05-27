namespace Transcriptor.Api.Features.TranscriptionJobs.Exceptions;

public sealed class ResourceNotFoundException(string message) : Exception(message);
