var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new HealthResponse("healthy")))
    .WithName("Health");

app.Run();

public record HealthResponse(string Status);
