using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DocsAi.Api.Tests.Common;

/// <summary>
/// Starts the API once and shares one HttpClient across every test in the "Api" collection.
/// The real Bedrock chat client is replaced by <see cref="FakeChatClient"/>.
/// </summary>
public sealed class ApiFixture : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiFixture()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.Replace(ServiceDescriptor.Singleton<IChatClient>(ChatClient))));

        Client = _factory.CreateClient();
    }

    public HttpClient Client { get; }

    public FakeChatClient ChatClient { get; } = new();

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
