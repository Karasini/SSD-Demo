using Transcriptor.Api.Domain;
using Transcriptor.Api.Features.TranscriptionJobs.DeleteTranscriptionJob.Dtos;
using Transcriptor.Api.Infrastructure.DI;
using Transcriptor.Api.Infrastructure.Persistence;
using Transcriptor.Api.Infrastructure.Storage;
using Transcriptor.Api.Infrastructure.Transcription;

namespace Transcriptor.Api.Features.TranscriptionJobs.DeleteTranscriptionJob.Commands;

public interface IDeleteTranscriptionJobCommand : ICommand
{
    /// <returns>True if deleted; false if job was not found.</returns>
    Task<bool> ExecuteAsync(DeleteTranscriptionJobRequest request, CancellationToken cancellationToken = default);
}

public sealed class DeleteTranscriptionJobCommand(
    ITranscriptionJobRepository repository,
    IObjectStorage objectStorage,
    TranscriptionWorkerClient workerClient) : IDeleteTranscriptionJobCommand
{
    public async Task<bool> ExecuteAsync(
        DeleteTranscriptionJobRequest request,
        CancellationToken cancellationToken = default)
    {
        var job = await repository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null)
        {
            return false;
        }

        await ExecuteDeletionAsync(job, repository, objectStorage, workerClient, cancellationToken);
        return true;
    }

    internal static async Task ExecuteDeletionAsync(
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
