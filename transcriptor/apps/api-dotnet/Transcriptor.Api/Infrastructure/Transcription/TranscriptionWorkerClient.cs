using System.Net.Http.Json;
using System.Text.Json;

namespace Transcriptor.Api.Infrastructure.Transcription;

public record RunTranscriptionJobRequest(
    Guid JobId,
    string MediaDownloadUrl,
    string FileName,
    string ContentType);

public class TranscriptionWorkerClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TranscriptionWorkerClient> _logger;
    private readonly string _apiKey;
    private const int MaxAttempts = 5;

    public TranscriptionWorkerClient(HttpClient httpClient, IConfiguration configuration, ILogger<TranscriptionWorkerClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["InternalApi:ApiKey"] ?? "dev-internal-key";
        var baseUrl = configuration["Worker:BaseUrl"] ?? "http://localhost:8000";
        _httpClient.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    }

    public async Task TriggerRunWithRetryAsync(RunTranscriptionJobRequest request, CancellationToken cancellationToken = default)
    {
        var delay = TimeSpan.FromSeconds(2);
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                using var message = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"internal/v1/jobs/{request.JobId}/run");
                message.Headers.Add("X-Internal-Api-Key", _apiKey);
                message.Content = JsonContent.Create(request, options: JsonOptions);
                var response = await _httpClient.SendAsync(message, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }

                _logger.LogWarning(
                    "Worker trigger attempt {Attempt} returned {StatusCode}",
                    attempt,
                    response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Worker trigger attempt {Attempt} failed", attempt);
            }

            if (attempt < MaxAttempts)
            {
                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 30));
            }
        }

        throw new InvalidOperationException("Worker unavailable");
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
