namespace Transcriptor.Api.Features.TranscriptionJobs.ListTranscriptionJobs.Dtos;

public record ListTranscriptionJobsRequest(int Page = 1, int PageSize = 50);
