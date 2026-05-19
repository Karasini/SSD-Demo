using Microsoft.AspNetCore.Mvc;
using Transcriptor.Api.Features.TranscriptionJobs.Commands;
using Transcriptor.Api.Features.TranscriptionJobs.Dtos;
using Transcriptor.Api.Features.TranscriptionJobs.Handlers;
using Transcriptor.Api.Features.TranscriptionJobs.Queries;

namespace Transcriptor.Api.Features.TranscriptionJobs;

public static class TranscriptionJobsEndpoints
{
    public static IEndpointRouteBuilder MapTranscriptionJobsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/transcription-jobs");

        group.MapGet("/", ListJobs);
        group.MapGet("/{id:guid}", GetJobById);
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
            new ListTranscriptionJobsQuery(page ?? 1, pageSize ?? 50),
            cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetJobById(
        Guid id,
        IGetTranscriptionJobByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var job = await handler.HandleAsync(new GetTranscriptionJobByIdQuery(id), cancellationToken);
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
                new CreateTranscriptionJobCommand(stream, file.FileName, file.ContentType, file.Length),
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

    private static async Task<IResult> UpdateJobStatus(
        Guid id,
        UpdateTranscriptionJobRequestDto body,
        IUpdateTranscriptionJobStatusHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.HandleAsync(
                new UpdateTranscriptionJobStatusCommand(
                    id,
                    body.Status,
                    body.TranscriptText,
                    body.DetectedLanguage,
                    body.FailureReason),
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
}
