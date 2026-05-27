using Microsoft.AspNetCore.Mvc;
using Transcriptor.Api.Features.TranscriptionJobs.BulkDeleteTranscriptionJobs;
using Transcriptor.Api.Features.TranscriptionJobs.BulkDeleteTranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.CreateTranscriptionJob;
using Transcriptor.Api.Features.TranscriptionJobs.CreateTranscriptionJob.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.CreateTranscriptionJob.Exceptions;
using Transcriptor.Api.Features.TranscriptionJobs.DeleteTranscriptionJob;
using Transcriptor.Api.Features.TranscriptionJobs.DeleteTranscriptionJob.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.Exceptions;
using Transcriptor.Api.Features.TranscriptionJobs.GetTranscriptionJobById;
using Transcriptor.Api.Features.TranscriptionJobs.GetTranscriptionJobById.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.ListTranscriptionJobs;
using Transcriptor.Api.Features.TranscriptionJobs.ListTranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.RenameSpeaker;
using Transcriptor.Api.Features.TranscriptionJobs.RenameSpeaker.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.UpdateTranscriptionJobStatus;
using Transcriptor.Api.Features.TranscriptionJobs.UpdateTranscriptionJobStatus.Dtos;

namespace Transcriptor.Api.Features.TranscriptionJobs;

public static class TranscriptionJobsEndpoints
{
    public static IEndpointRouteBuilder MapTranscriptionJobsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/transcription-jobs");

        group.MapGet("/", ListJobs);
        group.MapGet("/{id:guid}", GetJobById);
        group.MapPatch("/{id:guid}/speakers/{speakerId}", RenameSpeaker);
        group.MapDelete("/{id:guid}", DeleteJob);
        group.MapPost("/bulk-delete", BulkDeleteJobs);
        group.MapPost("/", CreateJob)
            .DisableAntiforgery();

        var internalGroup = app.MapGroup("/api/internal/v1/transcription-jobs");
        internalGroup.MapPatch("/{id:guid}", UpdateJobStatus);

        return app;
    }

    private static async Task<IResult> ListJobs(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        IListTranscriptionJobsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListTranscriptionJobsRequest(page ?? 1, pageSize ?? 50),
            cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetJobById(
        Guid id,
        IGetTranscriptionJobByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var job = await handler.HandleAsync(new GetTranscriptionJobByIdRequest(id), cancellationToken);
        return job is null ? Results.NotFound() : Results.Ok(job);
    }

    private static async Task<IResult> CreateJob(
        HttpRequest request,
        ICreateTranscriptionJobHandler handler,
        CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["file"] = ["File is required."]
            });
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files.GetFile("file");
        if (file is null || file.Length == 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["file"] = ["File is required."]
            });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await handler.HandleAsync(
                new CreateTranscriptionJobRequest(stream, file.FileName, file.ContentType, file.Length),
                cancellationToken);
            return Results.Created($"/api/v1/transcription-jobs/{result.Id}", result);
        }
        catch (PayloadTooLargeException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status413PayloadTooLarge);
        }
        catch (ValidationException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["file"] = [ex.Message]
            });
        }
    }

    private static async Task<IResult> DeleteJob(
        Guid id,
        IDeleteTranscriptionJobHandler handler,
        CancellationToken cancellationToken)
    {
        var deleted = await handler.HandleAsync(new DeleteTranscriptionJobRequest(id), cancellationToken);
        if (!deleted)
        {
            return Results.Problem(
                detail: "Transcription job was not found.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found");
        }

        return Results.NoContent();
    }

    private static async Task<IResult> BulkDeleteJobs(
        BulkDeleteTranscriptionJobsRequestDto body,
        IBulkDeleteTranscriptionJobsHandler handler,
        CancellationToken cancellationToken)
    {
        if (body.Ids is null || body.Ids.Count == 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["ids"] = ["At least one job id is required."]
            });
        }

        try
        {
            var result = await handler.HandleAsync(
                new BulkDeleteTranscriptionJobsRequest(body.Ids),
                cancellationToken);
            return Results.Ok(result);
        }
        catch (ValidationException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["ids"] = [ex.Message]
            });
        }
    }

    private static async Task<IResult> UpdateJobStatus(
        Guid id,
        UpdateTranscriptionJobRequestDto body,
        IUpdateTranscriptionJobStatusHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.HandleAsync(
                new UpdateTranscriptionJobStatusRequest(
                    id,
                    body.Status,
                    body.TranscriptText,
                    body.DetectedLanguage,
                    body.FailureReason,
                    body.Segments),
                cancellationToken);

            return result is null ? Results.NotFound() : Results.Ok(result);
        }
        catch (ValidationException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["status"] = [ex.Message]
            });
        }
    }

    private static async Task<IResult> RenameSpeaker(
        Guid id,
        string speakerId,
        RenameSpeakerRequestDto body,
        IRenameSpeakerHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.HandleAsync(
                new RenameSpeakerRequest(id, speakerId, body.DisplayName),
                cancellationToken);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }
        catch (ValidationException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["displayName"] = [ex.Message]
            });
        }
    }
}
