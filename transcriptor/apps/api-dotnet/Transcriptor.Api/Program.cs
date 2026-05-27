using Microsoft.EntityFrameworkCore;
using Transcriptor.Api;
using Transcriptor.Api.Infrastructure.DI;
using Transcriptor.Api.Infrastructure.Persistence;
using Transcriptor.Api.Infrastructure.Security;
using Transcriptor.Api.Infrastructure.Storage;
using Transcriptor.Api.Infrastructure.Transcription;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Host=localhost;Port=5432;Database=transcriptor;Username=transcriptor;Password=transcriptor";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<ITranscriptionJobRepository, TranscriptionJobRepository>();
builder.Services.AddSingleton<IObjectStorage, MinioObjectStorage>();
builder.Services.AddSingleton<WorkerTriggerQueue>();
builder.Services.AddHostedService<WorkerTriggerBackgroundService>();

builder.Services.AddHttpClient<TranscriptionWorkerClient>();

var assembly = typeof(Program).Assembly;
builder.Services.AddHandlers(assembly);
builder.Services.AddCommands(assembly);
builder.Services.AddQueries(assembly);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 2_147_483_648L;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseMiddleware<InternalApiKeyMiddleware>();
app.MapApplicationEndpoints();
app.Run();
