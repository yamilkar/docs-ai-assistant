using Amazon.BedrockRuntime;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace DocsAi.Api.Common.Ai;

public static class AiServiceCollectionExtensions
{
    /// <summary>
    /// Registers an <see cref="IChatClient"/> backed by Claude on Amazon Bedrock.
    /// AWS credentials come from the default chain (local profile, env vars or the ECS task role), never from config files.
    /// </summary>
    public static IServiceCollection AddClaudeOnBedrock(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<BedrockOptions>()
            .Bind(configuration.GetSection(BedrockOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDefaultAWSOptions(configuration.GetAWSOptions());
        services.AddAWSService<IAmazonBedrockRuntime>();

        services.AddChatClient(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BedrockOptions>>().Value;
                return sp.GetRequiredService<IAmazonBedrockRuntime>().AsIChatClient(options.ModelId);
            })
            .UseLogging();

        return services;
    }
}
