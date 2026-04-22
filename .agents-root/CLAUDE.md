# Workspace guide

This workspace is orchestrated by the **Todo** app (a kanban that dispatches agents against tickets).

## Todo API

The full and up-to-date API documentation is available at:
http://localhost:5230/api/docs

Consult it before interacting with the API. All ticket, comment, column, member, automation and run endpoints live there.

## Agents

Automated agents for this project live under `.agents/`:

- `.agents/preamble.md` — shared context injected into every agent run.
- `.agents/{agent}/SKILL.md` — per-agent instructions (editable).
- `.agents/{agent}/memory.md` — per-agent persistent memory (grows over runs).
- `.agents/automations.json` — trigger / condition / action pipelines.

The Todo background engine reads `automations.json` and launches `claude` CLI subprocesses in this working directory. Agents interact with the board via the Todo API above.
