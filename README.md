# docs-ai-assistant

AI-powered document Q&A API built with ASP.NET Core: RAG over your documents (pgvector), LLM tool calling, an MCP server for AI agents, Docker, AWS and GitHub Actions CI/CD.

## Run it

Requires the .NET 10 SDK.

```bash
dotnet test
dotnet run --project src/DocsAi.Api
curl http://localhost:5283/health
```

## Working with AI agents

This repo is built with AI coding agents (Claude Code). `CLAUDE.md` holds the rules the agent follows.
Notes on what the agent did well and what I corrected:

- _Day 1:_
