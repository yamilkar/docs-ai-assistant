# Design principles checklist (C# / ASP.NET Core, vertical slices)

For each principle: what a real finding looks like in this codebase, and the over-application to avoid. The second half matters as much as the first; a reviewer that flags every possible abstraction makes the code worse.

## Contents
1. Single responsibility
2. Open/closed
3. Liskov substitution
4. Interface segregation
5. Dependency inversion
6. DRY
7. YAGNI
8. KISS
9. Composition over inheritance
10. Modularity, low coupling, high cohesion

---

## 1. Single responsibility (S)

**Flag when** a class has more than one reason to change that is likely to happen independently. Common here:
- A controller action that builds the prompt, calls the LLM, queries pgvector and shapes the response. The controller should map HTTP to a service call and back.
- An `AskService` that both retrieves chunks and formats the prompt and parses the model output, when retrieval tuning and prompt changes are clearly separate work.

```csharp
// Finding: the controller owns retrieval, prompting and the LLM call.
[HttpPost]
public async Task<IActionResult> Ask(AskRequest req)
{
    var chunks = await _db.Chunks.FromSql($"...").ToListAsync();
    var prompt = $"Answer using: {string.Join("\n", chunks)} Q: {req.Question}";
    var reply = await _chat.GetResponseAsync(prompt);
    return Ok(new AskResponse(reply.Text));
}
```

**Don't flag** a small service that does one feature's job end to end in 40 readable lines. Splitting it into four classes is not SRP, it's ceremony.

## 2. Open/closed (O)

**Flag when** adding the next variant that is already planned (another document type, another model) requires editing a growing `switch` in several places.

**Don't flag** a two-case `if`. Introducing a strategy pattern for one or two cases that aren't expected to grow is YAGNI.

## 3. Liskov substitution (L)

Rare in this codebase. **Flag when** a subclass or interface implementation throws `NotSupportedException` for part of the contract, or silently ignores a parameter callers rely on (e.g. a fake vector store that ignores the patient filter, which would make tests pass on unscoped queries).

## 4. Interface segregation (I)

**Flag when** an interface grows methods that most consumers don't use, e.g. an `IDocumentStore` with upload, OCR, chunking, embedding and search, injected into a slice that only searches.

**Don't flag** a three-method interface used by one consumer.

## 5. Dependency inversion (D)

**Flag when** feature code depends directly on infrastructure that should sit behind a boundary:
- `new HttpClient()`, or the AWS Bedrock SDK used directly in a service instead of `IChatClient` / `IEmbeddingGenerator` from Microsoft.Extensions.AI (which `CLAUDE.md` names as the planned abstraction).
- `DateTime.UtcNow` in logic a test needs to control (use `TimeProvider`).
- Static helpers holding state or configuration (also a `CLAUDE.md` rule).

**Don't flag** a concrete in-process class injected directly when nothing needs to swap it. Not everything needs an interface.

## 6. DRY

**Flag** duplicated *knowledge*: the same rule, constant or decision in two places that must change together.
- Patient scoping implemented separately in two queries instead of one shared, tested filter.
- Chunk size, overlap, top-k, model ID or embedding dimensions as literals in more than one file.
- The same exception-to-HTTP mapping written in several controllers instead of one exception handler.
- Settings repeated in a `.csproj` that `Directory.Build.props` already sets for every project.

**Don't flag** similar-looking code in two vertical slices that encodes different decisions. Slices are meant to evolve independently; merging them couples features. Three similar lines are cheaper than the wrong abstraction.

## 7. YAGNI

**Flag** things built for a future that isn't in the PR's goal or in `CLAUDE.md`'s plan:
- A generic `IRepository<T>` over EF Core.
- Config options nothing reads, unused parameters, "for later" methods.
- Multi-provider plumbing when one provider is planned.
- Abstract base classes with one subclass.

**Don't flag** things `CLAUDE.md` explicitly plans (Bedrock, pgvector, MCP server, AWS deploy) when the PR is preparing for them in a proportionate way.

## 8. KISS

**Flag** code that is harder to follow than the problem requires: clever LINQ chains where a loop reads better, a custom retry loop instead of the standard resilience handler, reflection or source generation for a handful of cases, deeply nested conditionals that early returns would flatten.

## 9. Composition over inheritance

**Flag**:
- `BaseApiController` or `BaseService` classes that carry behavior (logging, patient lookup, error mapping). Prefer injected services, action filters, middleware, or the ASP.NET Core exception handler.
- Inheriting a framework or SDK type just to add a helper method.

**Don't flag** inheriting `ControllerBase`, `ProblemDetails`, or other framework extension points designed for it.

## 10. Modularity, low coupling, high cohesion

**Flag**:
- `using DocsAi.Api.Features.Documents...` inside `Features/Ask/` (a feature reaching into another feature). Shared needs go to `Common/` or through a deliberate contract.
- `Common/` acquiring feature-specific code ("AskHelpers" in `Common/`).
- A feature's pieces scattered outside its folder (its records in `Common/Models`, its service in a top-level `Services/` folder).
- Tests not mirroring the feature folder layout.
- One class touched for unrelated reasons across the PR, a sign its responsibilities aren't cohesive.

**Don't flag** a feature depending on `Common/` abstractions; that's the intended direction.
