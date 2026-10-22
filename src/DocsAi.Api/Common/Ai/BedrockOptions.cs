using System.ComponentModel.DataAnnotations;

namespace DocsAi.Api.Common.Ai;

/// <summary>
/// Which Claude model to call on Amazon Bedrock. Bound from the "Bedrock" config section.
/// </summary>
public sealed class BedrockOptions
{
    public const string SectionName = "Bedrock";

    [Required]
    public string ModelId { get; init; } = "";
}
