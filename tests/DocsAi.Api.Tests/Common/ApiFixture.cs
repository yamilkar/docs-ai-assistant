using Microsoft.AspNetCore.Mvc.Testing;

namespace DocsAi.Api.Tests.Common;

/// <summary>
/// Starts the API once and shares one HttpClient across every test in the "Api" collection.
/// </summary>
public sealed class ApiFixture : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();

    public ApiFixture() => Client = _factory.CreateClient();

    public HttpClient Client { get; }

    public void Dispose()
    {
        Client.Dispose();
        _factory.Dispose();
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "Api";
}
