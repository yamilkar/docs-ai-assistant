using DocsAi.Api.Common.Ai;
using DocsAi.Api.Features.Chat;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddClaudeOnBedrock(builder.Configuration);
builder.Services.AddScoped<ChatService>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new HealthResponse("healthy")))
    .WithName("Health");

app.MapControllers();

app.Run();

public record HealthResponse(string Status);
