using Transcriptor.Api.Domain;
using Transcriptor.Api.Features.TranscriptionJobs.Commands;
using Transcriptor.Api.Infrastructure.Persistence;
using Transcriptor.Api.Infrastructure.Storage;
using Transcriptor.Api.Infrastructure.Transcription;

namespace Transcriptor.Api.Features.TranscriptionJobs.Handlers;

public class TriggerTranscriptionJobHandler(
    ITranscriptionJobRepository repository,
    IObjectStorage objectStorage,
    TranscriptionWorkerClient workerClient) : ITriggerTranscriptionJobHandler
{
    public async Task HandleAsync(TriggerTranscriptionJobCommand command, CancellationToken cancellationToken = default)
    {
        var job = await repository.GetByIdAsync(command.JobId, cancellationToken)
            ?? throw new InvalidOperationException($"Job {command.JobId} not found.");

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
