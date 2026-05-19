using Transcriptor.Api.Common;
using Transcriptor.Api.Domain;
using Transcriptor.Api.Features.TranscriptionJobs.Commands;
using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Infrastructure.Persistence;
using Transcriptor.Api.Infrastructure.Storage;
using Transcriptor.Api.Infrastructure.Transcription;

namespace Transcriptor.Api.Features.TranscriptionJobs.Handlers;

public class CreateTranscriptionJobHandler(
    ITranscriptionJobRepository repository,
    IObjectStorage objectStorage,
    WorkerTriggerQueue triggerQueue) : ICreateTranscriptionJobHandler
{
    public async Task<TranscriptionJobDetailDto> HandleAsync(
        CreateTranscriptionJobCommand command,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(command.FileName);
        if (string.IsNullOrEmpty(extension) || !UploadConstraints.AllowedExtensions.Contains(extension))
        {
            throw new ValidationException(
                $"Unsupported file type. Allowed: {UploadConstraints.AllowedExtensionsDisplay}.");
        }

        if (command.FileSizeBytes > UploadConstraints.MaxFileSizeBytes)
        {
            throw new PayloadTooLargeException("Maximum file size is 2 GB.");
        }

        var contentTypeOk = UploadConstraints.AllowedContentTypePrefixes.Any(p =>
                command.ContentType.StartsWith(p, StringComparison.OrdinalIgnoreCase))
            || string.Equals(command.ContentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase);

        if (!contentTypeOk)
        {
            throw new ValidationException(
                $"Unsupported file type. Allowed: {UploadConstraints.AllowedExtensionsDisplay}.");
        }

        var jobId = Guid.NewGuid();
        var storageKey = $"transcription-jobs/{jobId}/source{extension}";
        var now = DateTimeOffset.UtcNow;

        await objectStorage.UploadAsync(storageKey, command.FileStream, command.ContentType, cancellationToken);

        var job = new TranscriptionJob
        {
            Id = jobId,
            FileName = command.FileName,
            FileSizeBytes = command.FileSizeBytes,
            ContentType = command.ContentType,
            Status = TranscriptionJobStatus.Queued,
            StorageKey = storageKey,
            CreatedAt = now,
            UpdatedAt = now
        };

        await repository.AddAsync(job, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await triggerQueue.EnqueueAsync(jobId, cancellationToken);

        return job.ToDetail();
    }
}

public class ValidationException(string message) : Exception(message);

public class PayloadTooLargeException(string message) : Exception(message);
