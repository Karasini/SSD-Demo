using Transcriptor.Api.Domain;
using Transcriptor.Api.Features.TranscriptionJobs.BulkDeleteTranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.DeleteTranscriptionJob.Commands;
using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.Exceptions;
using Transcriptor.Api.Infrastructure.DI;
using Transcriptor.Api.Infrastructure.Persistence;
using Transcriptor.Api.Infrastructure.Storage;
using Transcriptor.Api.Infrastructure.Transcription;

namespace Transcriptor.Api.Features.TranscriptionJobs.BulkDeleteTranscriptionJobs;

public interface IBulkDeleteTranscriptionJobsHandler
{
    Task<BulkDeleteTranscriptionJobsResponseDto> HandleAsync(
        BulkDeleteTranscriptionJobsRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class Handler(
    ITranscriptionJobRepository repository,
    IObjectStorage objectStorage,
    TranscriptionWorkerClient workerClient,
    ILogger<Handler> logger) : IBulkDeleteTranscriptionJobsHandler, IHandler
{
    private const int MaxBulkCount = 100;

    public async Task<BulkDeleteTranscriptionJobsResponseDto> HandleAsync(
        BulkDeleteTranscriptionJobsRequest request,
        CancellationToken cancellationToken = default)
    {
        var ids = request.Ids.Distinct().ToList();
        if (ids.Count == 0)
        {
            throw new ValidationException("At least one job id is required.");
        }

        if (ids.Count > MaxBulkCount)
        {
            throw new ValidationException($"At most {MaxBulkCount} job ids are allowed per request.");
        }

        var deletedIds = new List<Guid>();
        var failed = new List<BulkDeleteFailureItemDto>();

        foreach (var id in ids)
        {
            try
            {
                var job = await repository.GetByIdAsync(id, cancellationToken);
                if (job is null)
                {
                    failed.Add(new BulkDeleteFailureItemDto(
                        id,
                        "Transcription job was not found."));
                    continue;
                }

                await DeleteTranscriptionJobCommand.ExecuteDeletionAsync(
                    job, repository, objectStorage, workerClient, cancellationToken);
                deletedIds.Add(id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete transcription job {JobId}", id);
                failed.Add(new BulkDeleteFailureItemDto(
                    id,
                    "Could not remove this transcription. Try again."));
            }
        }

        return new BulkDeleteTranscriptionJobsResponseDto(deletedIds, failed);
    }
}
