# Migrer un `dispatcher.mjs` vers les automations Todo

Ce guide s'adresse à Claude : suis-le pour convertir un projet qui utilise
encore un `.agents/channel/dispatcher.mjs` vers le moteur d'automations intégré
à Todo. Aucun script n'est fourni — tu fais la migration pas à pas.

## Objectif

Remplacer `dispatcher.mjs` par :

1. Un fichier déclaratif `{WorkspacePath}/.agents/automations.json`.
2. Le champ `WorkspacePath` renseigné sur le projet Todo.
3. Le flag `IsAgent` coché sur chaque membre dispatchable (le skill est inféré par convention depuis `skills/{slug}.md`).

Le format de `.agents/channel/dispatch-state.json` est **identique** à l'ancien :
ne le modifie pas — les sessions Claude en cours reprendront sans rupture.

## Schéma d'`automations.json`

```json
{
  "dailyBudgetUsd": 70,
  "minDescriptionLength": 50,
  "automations": [
    {
      "id": "string-unique",
      "name": "Libellé optionnel",
      "enabled": true,
      "trigger": { "type": "...", ...params },
      "conditions": [ { "type": "...", ...params } ],
      "actions":    [ { "type": "...", ...params } ]
    }
  ]
}
```

### Triggers disponibles

| `type`             | Params principaux                                     | Usage type                                          |
|--------------------|-------------------------------------------------------|-----------------------------------------------------|
| `interval`         | `seconds: N` **ou** `cron: "min h d m j"`             | Polling régulier ou quotidien                       |
| `ticketInColumn`   | `column`, `assigneeSlug?`, `seconds`                  | Dispatch par assignation (cœur des `.mjs`)          |
| `gitCommit`        | `pollSeconds`                                         | Documentalist après un commit                       |
| `statusChange`     | `from?`, `to?`, `pollSeconds`, `debounceSeconds?`     | Learning / evaluator sur passage à `Done`           |
| `subTicketStatus`  | `parentColumn?`, `pollSeconds`, `debounceSeconds?`    | Producer qui se re-déclenche quand le CSV des sous-tickets change |
| `boardIdle`        | `idleColumns[]` (défaut Done/OwnerReview), `pollSeconds` | CEO wake A — se déclenche quand plus rien n'est actif |
| `agentInactivity`  | `minutesIdle`, `pollSeconds`                          | CEO wake B — aucun agent dispatché depuis N minutes |

### Conditions

| `type`                  | Params                                           |
|-------------------------|--------------------------------------------------|
| `ticketInColumn`        | `column`, `assigneeSlug?`                        |
| `noPendingTickets`      | `assigneeSlug?`, `columns?` (défaut `["Todo","InProgress"]`) |
| `minDescriptionLength`  | `length` (défaut 50)                             |

### Actions

| `type`            | Params                                                                                        |
|-------------------|-----------------------------------------------------------------------------------------------|
| `runClaudeSkill`  | `skillFile` (relatif à `.agents/`), `agentName?`, `maxTurns`, `concurrencyGroup?`, `mutuallyExclusiveWith[]`, `onStart.moveTo?`, `env?`, `model?`, `context?` |
| `moveTicketStatus`| `to`                                                                                          |

### Concurrence (`runClaudeSkill`)

- `concurrencyGroup` : au plus un run actif par groupe à la fois. Nomme-le librement (`"code"`, `"ceo"`, `"producer"`). Si absent, le nom de l'agent sert de groupe.
- `mutuallyExclusiveWith` : liste de groupes qu'on **bloque** pendant qu'on tourne. Sert à reproduire le `CEO bloque tout` de Lain, ou `producer` pousse tous les autres à attendre.
- Dédup implicite : pas de second run actif sur `(agent, ticketId)`.

## Mapping `.mjs` → automations

| Motif JS                                                                     | Équivalent JSON                                                |
|------------------------------------------------------------------------------|----------------------------------------------------------------|
| `setInterval(pollOnce, 30_000)` + `MEMBER_TO_AGENT[ticket.assignedTo]`       | `trigger: ticketInColumn` avec `assigneeSlug` par membre (skill file inféré depuis `skills/{slug}.md`) |
| `if (isTextAgent(name)) ... else if (codeAgentRunning) return`              | `concurrencyGroup: "code"` sur les actions non-textuelles      |
| `setInterval(pollGit, 60_000)` + `documentalist`                             | `trigger: gitCommit` + action `runClaudeSkill: skills/documentalist.md` |
| `setInterval(pollGroomer, 300_000)` + backlog <50 chars                      | `trigger: interval seconds=300` + condition `minDescriptionLength` (ou déroger à l'inverse — cf. Lain qui gate tout dispatch) |
| `setInterval(pollJanitor, 24h)` + aucun ticket code pending                  | `trigger: interval cron="0 3 * * *"` + `condition: noPendingTickets assigneeSlug="programmer"` |
| `setInterval(pollEvaluator, 30s)` debounce 90min sur Done/Review             | `trigger: statusChange to="Done" debounceSeconds=5400`         |
| CEO wake A (idle board) — Lain                                               | `trigger: boardIdle` avec `idleColumns: ["Done","OwnerReview"]` |
| CEO wake B (inactivity timeout) — Lain                                       | `trigger: agentInactivity` avec `minutesIdle: 45`              |
| `producer.lastSubStatuses` (Lain)                                            | `trigger: subTicketStatus` avec `parentColumn: "InProgress"` et `debounceSeconds` |
| Budget journalier (`DAILY_BUDGET_USD`)                                       | Racine : `"dailyBudgetUsd": 70` — bloque les non-CEO au-delà  |
| Seuil de grooming (`<50 chars` description)                                  | Racine : `"minDescriptionLength": 50` — bloque le dispatch    |

## Procédure de migration

1. **Lire `dispatcher.mjs`** du projet cible. Identifie :
   - la map `MEMBER_TO_AGENT` (→ membres à marquer `IsAgent`, avec slug = valeur de la map côté droit).
   - les `setInterval` / `setTimeout` et leurs durées.
   - les `TEXT_AGENTS` / `CODE_AGENTS` / locks (→ `concurrencyGroup`).
   - les chemins `SKILLS_DIR` et `.agents/skills/*.md` existants.

2. **Configurer le projet Todo**.
   Récupère le `slug` du projet et pose :
   ```bash
   curl -X PATCH http://localhost:5230/api/projects/{slug} \
        -H "Content-Type: application/json" \
        -d '{"workspacePath": "D:\\\\Sources\\\\Ekioo\\\\Aekan"}'
   ```
   (Ou ouvre `Board → ⚙️` dans l'UI et saisis le chemin.)

3. **Flaguer les membres agents** via `PATCH /api/projects/{slug}/members/{id}` avec `{ "isAgent": true }`, ou via la page **Paramètres** (case à cocher "Agent"). Par convention, le skill utilisé est `.agents/skills/{member.slug}.md`. Si le fichier manque, l'UI le signale par un ⚠.
   Pour un nom Aekan type "3D Artist" → slug `3d-artist` → `skills/3d-artist.md`. Si ton `.mjs` avait des alias (ex. `gameplay-programmer → programmer`), crée un membre dédié ou fais pointer tous tes tickets sur le slug unique (`programmer`).

4. **Écrire `automations.json`** à `{workspace}/.agents/automations.json`.
   Tu peux t'inspirer des exemples ci-dessous.

5. **Conserver `dispatch-state.json`** tel quel. Le moteur Todo lit exactement les mêmes clés (`_sessions`, `_lastProcessedCommit`, `_ticketSnapshot`, `{agent}.lastDispatched`) : les sessions Claude reprennent en `--resume`.

6. **Arrêter l'ancien `node dispatcher.mjs`**. Lancer Todo.Web suffit maintenant.

7. **Vérifier** dans `.agents/channel/debug.log` que les dispatches reprennent dans les 30 s.

## Exemple complet : BiggerInside (minimal, 2 agents)

```json
{
  "automations": [
    {
      "id": "producer-poll",
      "trigger": { "type": "ticketInColumn", "column": "Todo", "assigneeSlug": "producer", "seconds": 30 },
      "actions": [
        { "type": "runClaudeSkill", "skillFile": "skills/producer.md",
          "concurrencyGroup": "producer",
          "onStart": { "moveTo": "InProgress" } }
      ]
    },
    {
      "id": "developer-poll",
      "trigger": { "type": "ticketInColumn", "column": "Todo", "assigneeSlug": "developer", "seconds": 30 },
      "actions": [
        { "type": "runClaudeSkill", "skillFile": "skills/developer.md",
          "concurrencyGroup": "code",
          "onStart": { "moveTo": "InProgress" } }
      ]
    },
    {
      "id": "developer-inprogress",
      "trigger": { "type": "ticketInColumn", "column": "InProgress", "assigneeSlug": "developer", "seconds": 30 },
      "actions": [
        { "type": "runClaudeSkill", "skillFile": "skills/developer.md",
          "concurrencyGroup": "code" }
      ]
    }
  ]
}
```

## Exemple complet : Aekan (12 agents, doc sur commit, janitor quotidien)

```json
{
  "automations": [
    {
      "id": "text-producer",
      "trigger": { "type": "ticketInColumn", "column": "Todo", "assigneeSlug": "producer", "seconds": 30 },
      "actions": [
        { "type": "runClaudeSkill", "skillFile": "skills/producer.md",
          "concurrencyGroup": "producer",
          "onStart": { "moveTo": "InProgress" } }
      ]
    },
    {
      "id": "text-game-designer",
      "trigger": { "type": "ticketInColumn", "column": "Todo", "assigneeSlug": "game-designer", "seconds": 30 },
      "actions": [
        { "type": "runClaudeSkill", "skillFile": "skills/game-designer.md",
          "concurrencyGroup": "game-designer",
          "onStart": { "moveTo": "InProgress" } }
      ]
    },
    {
      "id": "code-programmer",
      "trigger": { "type": "ticketInColumn", "column": "Todo", "assigneeSlug": "programmer", "seconds": 30 },
      "actions": [
        { "type": "runClaudeSkill", "skillFile": "skills/programmer.md",
          "concurrencyGroup": "code",
          "onStart": { "moveTo": "InProgress" } }
      ]
    },
    {
      "id": "code-3d-artist",
      "trigger": { "type": "ticketInColumn", "column": "Todo", "assigneeSlug": "3d-artist", "seconds": 30 },
      "actions": [
        { "type": "runClaudeSkill", "skillFile": "skills/3d-artist.md",
          "concurrencyGroup": "code",
          "onStart": { "moveTo": "InProgress" } }
      ]
    },
    {
      "id": "documentalist-on-commit",
      "trigger": { "type": "gitCommit", "pollSeconds": 60 },
      "actions": [
        { "type": "runClaudeSkill", "skillFile": "skills/documentalist.md",
          "concurrencyGroup": "code" }
      ]
    },
    {
      "id": "code-janitor-daily",
      "trigger": { "type": "interval", "cron": "0 3 * * *" },
      "conditions": [ { "type": "noPendingTickets", "assigneeSlug": "programmer" } ],
      "actions": [
        { "type": "runClaudeSkill", "skillFile": "skills/code-janitor.md",
          "concurrencyGroup": "code" }
      ]
    },
    {
      "id": "learning-on-done",
      "trigger": { "type": "statusChange", "to": "Done", "pollSeconds": 30 },
      "actions": [
        { "type": "runClaudeSkill", "skillFile": "skills/learning.md",
          "context": "lastAssignee" }
      ]
    }
  ]
}
```

Reproduis le même schéma pour tous les skills présents dans `.agents/skills/`.
Chaque `id` doit être unique. Ajuste `concurrencyGroup` :

- `"producer"` / `"ceo"` → exclusif à eux-mêmes (un producer à la fois, CEO seul).
- `"code"` → partagé par tous les agents qui éditent du code (un seul à la fois).
- pas de groupe (`null`) → parallélisme illimité (rare).

## Exemple : Lain (15 agents, budget, producer avec sous-tickets, CEO wake)

```json
{
  "dailyBudgetUsd": 70,
  "minDescriptionLength": 50,
  "automations": [
    {
      "id": "producer-subtickets",
      "trigger": { "type": "subTicketStatus", "parentColumn": "InProgress", "pollSeconds": 30, "debounceSeconds": 900 },
      "actions": [
        { "type": "runClaudeSkill", "skillFile": "skills/producer.md",
          "concurrencyGroup": "producer", "mutuallyExclusiveWith": ["ceo"] }
      ]
    },
    {
      "id": "ceo-wake-idle",
      "trigger": { "type": "boardIdle", "idleColumns": ["Done", "OwnerReview"], "pollSeconds": 60 },
      "actions": [
        { "type": "runClaudeSkill", "skillFile": "skills/lain.md",
          "concurrencyGroup": "ceo", "mutuallyExclusiveWith": ["code", "producer"] }
      ]
    },
    {
      "id": "ceo-wake-inactivity",
      "trigger": { "type": "agentInactivity", "minutesIdle": 45, "pollSeconds": 60 },
      "actions": [
        { "type": "runClaudeSkill", "skillFile": "skills/lain.md",
          "concurrencyGroup": "ceo", "mutuallyExclusiveWith": ["code", "producer"] }
      ]
    },
    {
      "id": "web-dev-poll",
      "trigger": { "type": "ticketInColumn", "column": "Todo", "assigneeSlug": "web-dev", "seconds": 30 },
      "actions": [
        { "type": "runClaudeSkill", "skillFile": "skills/web-dev.md",
          "concurrencyGroup": "code",
          "onStart": { "moveTo": "InProgress" } }
      ]
    }
    /* …même schéma pour copywriter, content-creator, growth-marketer, designer,
       community-manager, market-researcher, etc. */
  ]
}
```

Avec `dailyBudgetUsd` renseigné, l'engine bloque automatiquement les dispatches non-CEO quand la somme du `cost-log.jsonl` journalier dépasse le seuil. Le seuil `minDescriptionLength` gate les actions ciblant un ticket dont la description est trop courte.

## Checklist après migration

- [ ] `automations.json` existe dans `{workspace}/.agents/` et est valide JSON.
- [ ] Champ `Project.WorkspacePath` = chemin absolu du repo cible.
- [ ] Chaque membre du projet a un `Skill` correspondant à un fichier `.agents/skills/{skill}.md`.
- [ ] `dispatcher.mjs` n'est plus lancé (process node tué).
- [ ] Todo.Web tourne et les logs `.agents/channel/debug.log` affichent des dispatches.
- [ ] Créer un ticket test assigné à un membre avec skill → il doit passer `InProgress` dans les 30 s.
- [ ] `dispatch-state.json._sessions` voit ses entrées mises à jour (sessions Claude resumed sans trou).

Si un agent ne se lance pas, vérifier :
- `.agents/skills/{skill}.md` existe bien.
- Le membre est bien assigné à ce skill (`GET /api/projects/{slug}/members`).
- L'`automations.json` est chargé (`POST /api/projects/{slug}/automations/reload`).
- Aucun run actif ne bloque via concurrence (`GET /api/projects/{slug}/runs`).
