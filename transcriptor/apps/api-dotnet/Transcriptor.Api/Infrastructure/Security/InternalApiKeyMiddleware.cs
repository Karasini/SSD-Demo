namespace Transcriptor.Api.Infrastructure.Security;

public class InternalApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private readonly string _expectedKey = configuration["InternalApi:ApiKey"] ?? "dev-internal-key";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api/internal"))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-Internal-Api-Key", out var provided)
            || provided != _expectedKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { title = "Unauthorized", status = 401 });
            return;
        }

        await next(context);
    }
}
