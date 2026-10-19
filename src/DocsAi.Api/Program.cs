var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new HealthResponse("healthy")))
    .WithName("Health");

app.MapControllers();

app.Run();

public record HealthResponse(string Status);
