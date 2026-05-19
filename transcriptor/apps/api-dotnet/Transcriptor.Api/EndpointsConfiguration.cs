using Transcriptor.Api.Features.TranscriptionJobs;

namespace Transcriptor.Api;

public static class EndpointsConfiguration
{
    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        app.MapTranscriptionJobsEndpoints();
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
        return app;
    }
}
