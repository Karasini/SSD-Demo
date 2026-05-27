using Transcriptor.Api.Features.TranscriptionJobs.CreateTranscriptionJob.Commands;
using Transcriptor.Api.Features.TranscriptionJobs.CreateTranscriptionJob.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Infrastructure.DI;
using Transcriptor.Api.Infrastructure.Transcription;

namespace Transcriptor.Api.Features.TranscriptionJobs.CreateTranscriptionJob;

public interface ICreateTranscriptionJobHandler
{
    Task<TranscriptionJobDetailDto> HandleAsync(
        CreateTranscriptionJobRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class Handler(
    ICreateTranscriptionJobCommand createCommand,
    WorkerTriggerQueue triggerQueue) : ICreateTranscriptionJobHandler, IHandler
{
    public async Task<TranscriptionJobDetailDto> HandleAsync(
        CreateTranscriptionJobRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await createCommand.ExecuteAsync(request, cancellationToken);
        await triggerQueue.EnqueueAsync(result.Id, cancellationToken);
        return result;
    }
}
