namespace Transcriptor.Api.Features.TranscriptionJobs.BulkDeleteTranscriptionJobs.Dtos;

public record BulkDeleteTranscriptionJobsRequest(IReadOnlyList<Guid> Ids);
