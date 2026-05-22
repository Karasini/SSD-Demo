using Transcriptor.Api.Domain;
using Transcriptor.Api.Features.TranscriptionJobs.Commands;
using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Infrastructure.Persistence;
using Transcriptor.Api.Infrastructure.Storage;
using Transcriptor.Api.Infrastructure.Transcription;

namespace Transcriptor.Api.Features.TranscriptionJobs.Handlers;

public class DeleteTranscriptionJobHandler(
    ITranscriptionJobRepository repository,
    IObjectStorage objectStorage,
    TranscriptionWorkerClient workerClient) : IDeleteTranscriptionJobHandler
{
    public async Task<bool> HandleAsync(
        DeleteTranscriptionJobCommand command,
        CancellationToken cancellationToken = default)
    {
        var job = await repository.GetByIdAsync(command.JobId, cancellationToken);
        if (job is null)
        {
            return false;
        }

        await TranscriptionJobDeletion.ExecuteAsync(job, repository, objectStorage, workerClient, cancellationToken);
        return true;
    }
}

public class BulkDeleteTranscriptionJobsHandler(
    ITranscriptionJobRepository repository,
    IObjectStorage objectStorage,
    TranscriptionWorkerClient workerClient,
    ILogger<BulkDeleteTranscriptionJobsHandler> logger) : IBulkDeleteTranscriptionJobsHandler
{
    private const int MaxBulkCount = 100;

    public async Task<BulkDeleteTranscriptionJobsResponseDto> HandleAsync(
        BulkDeleteTranscriptionJobsCommand command,
        CancellationToken cancellationToken = default)
    {
        var ids = command.Ids.Distinct().ToList();
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

                await TranscriptionJobDeletion.ExecuteAsync(
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

internal static class TranscriptionJobDeletion
{
    public static async Task ExecuteAsync(
        TranscriptionJob job,
        ITranscriptionJobRepository repository,
        IObjectStorage objectStorage,
        TranscriptionWorkerClient workerClient,
        CancellationToken cancellationToken)
    {
        if (TranscriptionJobStatus.IsActive(job.Status))
        {
            await workerClient.CancelJobAsync(job.Id, cancellationToken);
        }

        await objectStorage.DeleteAsync(job.StorageKey, cancellationToken);
        await repository.RemoveAsync(job, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
