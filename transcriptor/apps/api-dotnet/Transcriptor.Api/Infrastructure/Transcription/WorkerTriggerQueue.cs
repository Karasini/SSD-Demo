using System.Threading.Channels;
using Transcriptor.Api.Domain;
using Transcriptor.Api.Features.TranscriptionJobs.TriggerTranscriptionJob;
using Transcriptor.Api.Features.TranscriptionJobs.TriggerTranscriptionJob.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.UpdateTranscriptionJobStatus;
using Transcriptor.Api.Features.TranscriptionJobs.UpdateTranscriptionJobStatus.Dtos;

namespace Transcriptor.Api.Infrastructure.Transcription;

public class WorkerTriggerQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

    public ValueTask EnqueueAsync(Guid jobId, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(jobId, cancellationToken);

    public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken = default) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}

public class WorkerTriggerBackgroundService(
    WorkerTriggerQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<WorkerTriggerBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var jobId in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<ITriggerTranscriptionJobHandler>();
                await handler.HandleAsync(new TriggerTranscriptionJobRequest(jobId), stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to trigger worker for job {JobId}", jobId);
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var failHandler = scope.ServiceProvider
                        .GetRequiredService<IUpdateTranscriptionJobStatusHandler>();
                    await failHandler.HandleAsync(
                        new UpdateTranscriptionJobStatusRequest(
                            jobId,
                            TranscriptionJobStatus.Failed,
                            null,
                            null,
                            "Worker unavailable",
                            null),
                        stoppingToken);
                }
                catch (Exception failEx)
                {
                    logger.LogError(failEx, "Failed to mark job {JobId} as failed", jobId);
                }
            }
        }
    }
}
