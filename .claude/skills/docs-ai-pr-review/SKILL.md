---
name: docs-ai-pr-review
description: Review a pull request in the docs-ai-assistant RAG API (the .NET backend for the Mi Salud app) and post inline review comments on GitHub, checking SOLID, DRY, YAGNI, KISS, composition over inheritance, modularity, low coupling and high cohesion, plus the repo's CLAUDE.md rules, patient data safety and RAG/LLM error handling. Use this whenever Claude has opened or updated a PR in docs-ai-assistant and should review its own changes, and whenever the user asks to review, critique or "check the design of" a PR, branch or diff in this repo, even if they don't say "skill" or name the principles.
---

# docs-ai-assistant PR design review

You are reviewing a pull request in `yamilkar/docs-ai-assistant`, an ASP.NET Core (.NET 10) RAG API whose first client is Mi Salud, an app where patients upload medical results and ask questions about them by voice. The output is **inline comments on the PR**, one per finding, each tagged with a severity. You only flag; you never push fixes, approve, or request changes. Yamilka decides what to change.

Most of the time the code under review was written by Claude. That is exactly why this review exists: an author reviewing their own work tends to read what they meant rather than what they wrote. Read the diff as if a stranger wrote it. If you can spawn a subagent, run the review in one that gets only the PR number and this skill, not the conversation that produced the code.

## 1. Gather context

1. Find the PR. Use the number the user gives, or the PR for the current branch: `gh pr view --json number,title,body,headRefOid,baseRefName,files`.
2. Get the diff: `gh pr diff <N> > .claude/reviews/pr-<N>/pr.diff` (create the folder first). The posting script needs it. `.claude/reviews/` is in `.gitignore`, so review files never end up in a commit; never write them anywhere else in the repo.
3. Read `CLAUDE.md` at the repo root. It is the source of truth for layout and rules and may have changed since this skill was written; if it disagrees with this skill, follow `CLAUDE.md`.
4. Read every changed file **in full**, not just the hunks, plus the code it calls and the code that calls it. Many design problems (a feature reaching into another feature's folder, a second implementation of patient scoping) are invisible from the hunk alone.
5. If the branch is checked out locally, run `dotnet build` and `dotnet test`. A failing build or test is a blocker; anchor it to the most relevant changed line and paste the key error.

## 2. What to check

Work through each area below for every changed file. The reference `references/checklist.md` has concrete signs and C# examples for each principle; read it the first time you use this skill in a session.

### Design principles (the core of the review)

SOLID, DRY, YAGNI, KISS, composition over inheritance, modularity, low coupling, high cohesion.

These principles pull against each other. SOLID and DRY push toward more abstraction; YAGNI and KISS push toward less. A good review holds both. Before flagging a principle violation, ask: **what concrete cost does this cause now, or on the very next change already planned in `CLAUDE.md`?** If you can't name one, don't comment. "This could be more extensible" is not a finding; "the next provider swap means editing three controllers" is.

How the repo's architecture changes the usual reading:

- **Vertical slices are the modularity unit.** Each `Features/<Feature>/` folder owns its controller, records, service and data access. A feature referencing another feature's folder is a coupling finding. Shared code belongs in `Common/`, and `Common/` should hold things that are genuinely shared, not a dumping ground.
- **DRY is about knowledge, not text.** Two slices with similar-looking controllers are fine; slices are supposed to be independent. Flag duplication when the same *rule or decision* lives in two places and could drift: patient scoping, chunk size, the prompt template, the embedding model name, mapping of errors to HTTP responses.
- **Interfaces need a reason.** A one-implementation interface is justified at a boundary (LLM client, vector store, clock, anything a test must replace). An interface for a pure in-process helper is YAGNI.
- **Composition over inheritance.** Base controllers or base services carrying shared logic should usually be injected services, filters, or middleware instead.

### CLAUDE.md rules

Check the rules as written in `CLAUDE.md`. At the time of writing these include: every new endpoint has a test; tests use the shared `ApiFixture` via `[Collection(ApiCollection.Name)]` and never create their own factory or client; async all the way, with `CancellationToken` passed through; dependencies injected through DI, no static state; no secrets in code or config; test fixtures are synthetic; every query is scoped to one patient.

### Patient data safety

This API handles medical documents, so these findings are usually blockers:

- **Cross-patient access.** The patient identity must come from the authenticated context, not from a request body or query string the caller controls. Every vector search and every database query must filter by patient *in the query itself* (a SQL `WHERE`, a pgvector filter), never by fetching broadly and filtering in memory.
- **PII leaving where it should stay.** No logging of document text, extracted results, questions, prompts or LLM responses. Watch structured logging that serializes whole request/response objects. Don't send the LLM more of the patient's data than the question needs.
- **Prompt injection through documents.** Retrieved chunks are untrusted data. They should be clearly delimited in the prompt and the system prompt should say to treat them as content, not instructions. Document content must never be able to trigger a tool call that escapes the current patient's scope.
- **Real data in the repo.** Fixtures and sample files must be synthetic.

### RAG / LLM error handling

- Calls to Bedrock and to Postgres/pgvector have timeouts and honor the request's `CancellationToken`.
- Throttling and transient failures are retried with backoff (prefer the standard resilience pipeline over hand-rolled loops), and non-transient failures are not retried.
- Failures map to `ProblemDetails` responses that don't leak stack traces, prompts, or document content.
- **No-result paths are honest.** When retrieval returns nothing relevant, the API says it couldn't find the answer in the patient's documents instead of letting the model answer from general knowledge as if it came from their results.

## 3. Severity

Tag every comment with exactly one:

- **blocker**: must be fixed before merge. Bugs, failing build or tests, broken `CLAUDE.md` rules, any patient data safety issue, missing error handling that would crash or leak, a design problem that will force a rewrite of this PR's code on the next planned change.
- **suggestion**: a design issue with a concrete, nameable cost that isn't urgent.
- **nit**: small and cheap to fix. Post **at most five** nits per review; keep the ones that matter most. Too many nits bury the blockers.

Don't comment on anything the compiler already enforces (`TreatWarningsAsErrors` is on). Don't post praise or "looks good" comments; inline comments are for things to act on.

## 4. Write each comment

Each comment sits on the line it's about and reads on its own, because Yamilka will see it in the GitHub diff view without this context. Keep the shape:

```
**[severity] Principle**: one-line statement of the problem

Why it matters here, in one or two sentences: the concrete cost.

What to do instead, with a short code sketch when it helps.
```

`Principle` is the principle or check area, e.g. `Low coupling`, `YAGNI`, `Patient data safety`, `CLAUDE.md: tests`. When the fix is a small, exact replacement of the commented lines, use a GitHub ` ```suggestion ` block so it can be applied with one click; that's still flagging, since she chooses whether to apply it.

**Anchoring.** GitHub only accepts inline comments on lines that appear in the diff (added or unchanged context lines on the new side). If a finding is about something *missing* (no test for a new endpoint, no `CancellationToken`), anchor it to the line that introduces the thing missing it, such as the new action method. If a finding spans several lines, give `start_line` and `line`.

## 5. Post the review

Write the findings to `.claude/reviews/pr-<N>/findings.json`:

```json
{
  "comments": [
    {
      "path": "src/DocsAi.Api/Features/Ask/AskService.cs",
      "line": 42,
      "start_line": 38,
      "severity": "blocker",
      "principle": "Patient data safety",
      "title": "Vector search isn't filtered by patient",
      "body": "Why it matters...\n\nWhat to do instead..."
    }
  ]
}
```

`start_line` is optional. Then post with the bundled script, which checks every line against the diff, skips findings already posted on an earlier run of this review, and submits them all as one review with the `COMMENT` event:

```bash
python .claude/skills/docs-ai-pr-review/scripts/post_review.py --repo yamilkar/docs-ai-assistant --pr <N> \
  --findings .claude/reviews/pr-<N>/findings.json --diff .claude/reviews/pr-<N>/pr.diff
```

Add `--dry-run` to print what would be posted without posting. If the script rejects a line, fix the anchor and rerun; don't drop the finding.

If there are no findings, post nothing.

## 6. Report back

In chat, give a short summary: how many blockers, suggestions and nits were posted (and how many were skipped as already posted), the one or two findings that matter most in a sentence each, and the PR link. Don't repeat every comment; they're on the PR.
