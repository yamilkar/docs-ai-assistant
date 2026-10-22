# docs-ai-assistant

AI-powered document Q&A API built with ASP.NET Core: RAG over your documents (pgvector), LLM tool calling, an MCP server for AI agents, Docker, AWS and GitHub Actions CI/CD.

## Run it

Requires the .NET 10 SDK.

```bash
dotnet test
dotnet run --project src/DocsAi.Api
curl http://localhost:5283/health
```

## Chat with Claude on Amazon Bedrock

`POST /chat` sends a message to Claude on Amazon Bedrock through `Microsoft.Extensions.AI` (`IChatClient`).
Bedrock uses AWS credentials, not an API key, so nothing secret lives in the repo:

1. In the AWS console, enable access to the Claude model in Amazon Bedrock (region `us-east-1`).
2. Sign in locally with a profile: `aws configure sso --profile docs-ai`, then `aws sso login --profile docs-ai`.
3. Tell the API which profile to use (stored outside the repo with user secrets):

```bash
dotnet user-secrets set "AWS:Profile" "docs-ai" --project src/DocsAi.Api
dotnet run --project src/DocsAi.Api
curl -X POST http://localhost:5283/chat -H "Content-Type: application/json" -d '{"message":"¿Qué mide la hemoglobina glicosilada?"}'
```

The model id is `Bedrock:ModelId` in `appsettings.json`. If Bedrock asks for an inference profile in your region, use its id (for example with a `us.` prefix).
In production (ECS on Fargate) the task role provides the credentials.

## Working with AI agents

This repo is built with AI coding agents (Claude Code). `CLAUDE.md` holds the rules the agent follows.
Notes on what the agent did well and what I corrected:

- _Day 1:_
- _Day 2:_ added `/chat` with Microsoft.Extensions.AI and Claude on Bedrock. The plan said "API key in user secrets", but Bedrock authenticates with AWS credentials, so only the profile name goes in user secrets. Tests swap the real `IChatClient` for a fake one, so they never call AWS.
