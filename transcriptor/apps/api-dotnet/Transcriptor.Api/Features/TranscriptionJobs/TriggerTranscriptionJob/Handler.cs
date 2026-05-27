using Transcriptor.Api.Domain;
using Transcriptor.Api.Features.TranscriptionJobs.TriggerTranscriptionJob.Dtos;
using Transcriptor.Api.Infrastructure.DI;
using Transcriptor.Api.Infrastructure.Persistence;
using Transcriptor.Api.Infrastructure.Storage;
using Transcriptor.Api.Infrastructure.Transcription;

namespace Transcriptor.Api.Features.TranscriptionJobs.TriggerTranscriptionJob;

public interface ITriggerTranscriptionJobHandler
{
    Task HandleAsync(TriggerTranscriptionJobRequest request, CancellationToken cancellationToken = default);
}

public sealed class Handler(
    ITranscriptionJobRepository repository,
    IObjectStorage objectStorage,
    TranscriptionWorkerClient workerClient) : ITriggerTranscriptionJobHandler, IHandler
{
    public async Task HandleAsync(TriggerTranscriptionJobRequest request, CancellationToken cancellationToken = default)
    {
        var job = await repository.GetByIdAsync(request.JobId, cancellationToken)
            ?? throw new InvalidOperationException($"Job {request.JobId} not found.");

        if (TranscriptionJobStatus.IsTerminal(job.Status))
        {
            return;
        }

        var downloadUrl = await objectStorage.GetPresignedDownloadUrlAsync(
            job.StorageKey,
            TimeSpan.FromHours(2),
            cancellationToken);

        await workerClient.TriggerRunWithRetryAsync(
            new RunTranscriptionJobRequest(job.Id, downloadUrl, job.FileName, job.ContentType),
            cancellationToken);
    }
}
