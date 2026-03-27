# Todo Board API

Base URL: `http://localhost:5230`

## Projets

### Lister les projets
```
GET /api/projects
```
Response: `Project[]`

### Créer un projet
```
POST /api/projects
Content-Type: application/json

{"name": "Mon Projet"}
```
Response: `201 Created` + `Project`

### Détail d'un projet
```
GET /api/projects/{slug}
```
Response: `Project` ou `404`

## Tickets

### Lister les tickets d'un projet
```
GET /api/projects/{slug}/tickets
GET /api/projects/{slug}/tickets?status=InProgress
```
Statuts possibles: `Backlog`, `Todo`, `InProgress`, `Blocked`, `OwnerReview`, `Done`

Response: `Ticket[]`

### Créer un ticket
```
POST /api/projects/{slug}/tickets
Content-Type: application/json

{
  "title": "Implémenter le login",
  "description": "Ajouter OAuth2",
  "createdBy": "agent:claude",
  "status": "Backlog"
}
```
Champs optionnels: `description` (défaut: ""), `createdBy` (défaut: "owner"), `status` (défaut: "Backlog")

Response: `201 Created` + `Ticket`

### Détail d'un ticket (avec commentaires)
```
GET /api/projects/{slug}/tickets/{id}
```
Response: `Ticket` (inclut `comments[]`) ou `404`

### Supprimer un ticket
```
DELETE /api/projects/{slug}/tickets/{id}
```
Response: `204 No Content` ou `404`

### Déplacer un ticket
```
PATCH /api/projects/{slug}/tickets/{id}/status
Content-Type: application/json

{"status": "InProgress"}
```
Response: `Ticket` ou `404`

## Commentaires

### Ajouter un commentaire
```
POST /api/projects/{slug}/tickets/{id}/comments
Content-Type: application/json

{
  "content": "J'ai commencé l'implémentation",
  "author": "agent:claude"
}
```
Champ optionnel: `author` (défaut: "owner")

Response: `201 Created` + `Comment`

## Modèles

### Project
```json
{"id": 1, "name": "Mon Projet", "slug": "mon-projet", "createdAt": "2026-03-27T10:00:00Z"}
```

### Ticket
```json
{"id": 1, "title": "...", "description": "...", "status": "Backlog", "createdBy": "owner", "createdAt": "...", "updatedAt": "...", "comments": []}
```

### Comment
```json
{"id": 1, "ticketId": 1, "content": "...", "author": "agent:claude", "createdAt": "..."}
```

## Convention `createdBy` / `author`
- Owner: `"owner"`
- Agents: `"agent:{nom}"` (ex: `"agent:claude"`, `"agent:codex"`)

## OpenAPI
Spec complète disponible sur: `GET /openapi/v1.json`
