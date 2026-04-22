# KittyClaw — Claude guide

A Blazor Server + .NET 10 kanban that orchestrates agentic projects. Each project can have LLM members; a background `AutomationEngine` dispatches them as `claude` CLI subprocesses based on triggers (column changes, intervals, git commits, …).

## Run

```
cd KittyClaw.Web && dotnet watch --non-interactive
# → http://localhost:5230
dotnet test KittyClaw.Core.Tests
```

Keep the `dotnet watch` process running — it serves the UI and the automation engine. If `dotnet build` shows MSB3027 / MSB3021 file-lock errors, they are NOT compile errors; only `error CS####` matters.

## Repository layout

```
KittyClaw.Core/            Models, services, automation engine, embedded .agents/ template
KittyClaw.Core.Tests/      xUnit tests
KittyClaw.Web/             Blazor Server app + REST endpoints (Api/Endpoints.cs), components, CSS, JS
.agents/              Live agents used by KittyClaw itself (also embedded as template for other projects)
.agents-root/         Files copied to each initialized workspace's root (e.g. CLAUDE.md)
```

## Storage

- Project registry: `%APPDATA%/KittyClaw/registry.db` (SQLite).
- Per-project DB: `%APPDATA%/KittyClaw/projects/<slug>.db`.
- Run logs: `%APPDATA%/KittyClaw/runs/<run-id>/`.
- App settings (language, onboardingSeen): `%APPDATA%/KittyClaw/settings.json`.
- Agent memory and session state: `<workspace>/.agents/**`.

## Conventions

- **Inline SQLite migrations**: `CREATE TABLE IF NOT EXISTS` + `ALTER TABLE ADD COLUMN` in try/catch. No EF Migrations.
- **DTOs** are `record` types.
- **Services** are singletons injected via DI in `KittyClaw.Web/Program.cs`.
- **Blazor components**: `@rendermode InteractiveServer`, `[Parameter]`, `StateHasChanged()`. Prefer direct service calls over HTTP self-calls.
- **CSS** lives in a single `KittyClaw.Web/wwwroot/app.css`. **JS** in `KittyClaw.Web/wwwroot/js/`.
- **English everywhere**: code comments, commit messages, ticket content, `.agents/**`.

## Agent template embedding

`.agents/preamble.md`, `.agents/*/SKILL.md`, `.agents/*/memory.md` and `.agents/automations.json` are embedded into `KittyClaw.Core.dll` via `KittyClaw.Core.csproj` (`<EmbeddedResource>` items with `LogicalName` `KittyClaw.Core.AgentsTemplate/…`). `AgentsTemplateService` enumerates them and copies them into a target project's workspace on Initialize. Keep these files **generic** (no KittyClaw-specific stack references) since they are shipped as a starter template to other projects.

## API

Auto-generated at runtime from the OpenAPI spec. Read it live — do not rely on any committed snapshot:

- `http://localhost:5230/api/docs` (Markdown)
- `http://localhost:5230/openapi/v1.json` (JSON)
