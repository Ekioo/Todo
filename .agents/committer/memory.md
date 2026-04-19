# Memory — committer

## Leçons apprises

- `.agents/` est dans `.gitignore` → les fichiers agents ne sont jamais trackés par git. Ne pas tenter de les commiter.
- `git status .agents/` dit "clean" même si des fichiers existent dedans — ils sont ignorés.
- Pour poster un commentaire avec `curl`, utiliser `printf '...' | curl --data-binary @-` pour éviter les problèmes d'encodage UTF-8 sur Windows avec les apostrophes/accents dans le JSON inline.

## Patterns de succès

- Vérifier `git ls-files <dir>` pour confirmer qu'un dossier est bien tracké avant de conclure qu'il y a quelque chose à commiter. [1]
- Utiliser `printf '...' | curl --data-binary @-` pour les commentaires API afin d'éviter les erreurs d'encodage. [1]

## Anti-patterns

- Ne pas faire confiance à `git status <dir>` seul pour détecter si un répertoire contient du travail commitable — il peut être ignoré. [1]

## Préférences owner

## Métriques

- commits_créés [1]
- commits_rejetés [0]
- tickets_passés_done [2]
- conflits_résolus [0]
