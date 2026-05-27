using Transcriptor.Api.Common;
using Transcriptor.Api.Domain;
using Transcriptor.Api.Features.TranscriptionJobs.CreateTranscriptionJob.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.CreateTranscriptionJob.Exceptions;
using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.Exceptions;
using Transcriptor.Api.Infrastructure.DI;
using Transcriptor.Api.Infrastructure.Persistence;
using Transcriptor.Api.Infrastructure.Storage;

namespace Transcriptor.Api.Features.TranscriptionJobs.CreateTranscriptionJob.Commands;

public interface ICreateTranscriptionJobCommand : ICommand
{
    Task<TranscriptionJobDetailDto> ExecuteAsync(
        CreateTranscriptionJobRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateTranscriptionJobCommand(
    ITranscriptionJobRepository repository,
    IObjectStorage objectStorage) : ICreateTranscriptionJobCommand
{
    public async Task<TranscriptionJobDetailDto> ExecuteAsync(
        CreateTranscriptionJobRequest request,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(request.FileName);
        if (string.IsNullOrEmpty(extension) || !UploadConstraints.AllowedExtensions.Contains(extension))
        {
            throw new ValidationException(
                $"Unsupported file type. Allowed: {UploadConstraints.AllowedExtensionsDisplay}.");
        }

        if (request.FileSizeBytes > UploadConstraints.MaxFileSizeBytes)
        {
            throw new PayloadTooLargeException("Maximum file size is 2 GB.");
        }

        var contentTypeOk = UploadConstraints.AllowedContentTypePrefixes.Any(p =>
                request.ContentType.StartsWith(p, StringComparison.OrdinalIgnoreCase))
            || string.Equals(request.ContentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase);

        if (!contentTypeOk)
        {
            throw new ValidationException(
                $"Unsupported file type. Allowed: {UploadConstraints.AllowedExtensionsDisplay}.");
        }

        var jobId = Guid.NewGuid();
        var storageKey = $"transcription-jobs/{jobId}/source{extension}";
        var now = DateTimeOffset.UtcNow;

        await objectStorage.UploadAsync(storageKey, request.FileStream, request.ContentType, cancellationToken);

        var job = new TranscriptionJob
        {
            Id = jobId,
            FileName = request.FileName,
            FileSizeBytes = request.FileSizeBytes,
            ContentType = request.ContentType,
            Status = TranscriptionJobStatus.Queued,
            StorageKey = storageKey,
            CreatedAt = now,
            UpdatedAt = now
        };

        await repository.AddAsync(job, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return job.ToDetail();
    }
}
