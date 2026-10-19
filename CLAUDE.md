# docs-ai-assistant

ASP.NET Core Web API (.NET 10) that answers questions about documents using RAG.
First client: the "Mi Salud" app, where patients upload medical results (PDF and images) and ask about them by voice.

## Layout
- `src/DocsAi.Api` — the API (Minimal APIs).
- `tests/DocsAi.Api.Tests` — xUnit tests; endpoint tests use `WebApplicationFactory<Program>`.
- `DocsAi.slnx` — solution file. `Directory.Build.props` turns warnings into errors.

## Commands
- Build: `dotnet build`
- Test: `dotnet test`
- Run: `dotnet run --project src/DocsAi.Api`

## Rules
- Every new endpoint gets a test. Run `dotnet test` and make sure it passes before saying a task is done.
- Show a short plan before writing code for anything larger than a one-file change.
- Async all the way; inject dependencies through DI, no static state.
- No secrets in code or config files: use `dotnet user-secrets` locally and AWS Secrets Manager in production.
- Never commit real medical documents or patient data. Test fixtures are synthetic.
- Every query is scoped to one patient; never return another patient's data.
- Small commits with clear messages.

## Stack (planned)
LLM: Claude on Amazon Bedrock via Microsoft.Extensions.AI. Vector store: PostgreSQL + pgvector (Docker locally, Amazon RDS in production). Deploy: Docker, Amazon ECS on Fargate, AWS CDK in C#, GitHub Actions.
