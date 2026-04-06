# Todo

A kanban board for managing projects with tickets, columns, labels, and activity tracking — designed to be driven by AI agents via its API.

## Tech Stack

- **.NET 10** / **Blazor Server** (interactive SSR)
- **SQLite** via Entity Framework Core (one DB per project)
- **OpenAPI** with auto-generated Markdown docs

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run

```bash
cd Todo.Web
dotnet run --launch-profile http
```

The app is available at **http://localhost:5230**.

For hot reload during development:

```bash
dotnet watch --launch-profile http
```

### Data Storage

All data is stored locally in `%APPDATA%/TodoApp/`:

- `registry.db` — project registry
- `projects/{slug}.db` — per-project database (tickets, comments, labels, columns, members)
- `uploads/` — uploaded images

## Project Structure

| Project | Description |
|---|---|
| **Todo.Core** | Domain models, EF Core contexts, and business services |
| **Todo.Web** | Blazor Server UI + REST API |

## API

All endpoints are under `/api`. See [API.md](API.md) for full documentation.

OpenAPI spec available at `GET /openapi/v1.json` and human-readable docs at `GET /api/docs`.

## Conventions

- **Author format**: `"owner"` for the human user, `"agent:{name}"` for AI agents
- **Priority levels**: `Idea`, `NiceToHave`, `Required`, `Critical`
- **Default column**: `Backlog`

## UI Features

- Kanban board with drag-and-drop
- Ticket detail panel with comments and activity timeline
- Markdown rendering with `@mention` and `#ticket` reference support
- Advanced search syntax: `#42`, `@owner`, `>date`, `priority:critical`, `label:bug`, `by:owner`
- Label and member management
- Image upload in descriptions and comments
