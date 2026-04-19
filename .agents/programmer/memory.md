# Memory — programmer

## Leçons apprises

- curl avec des caractères spéciaux (apostrophes, accents) dans le JSON inline provoque des erreurs d'encodage UTF-8 sur Windows — toujours utiliser un heredoc ou un fichier temporaire [+2]

- Soigner le commentaire de livraison : accents, lisibilité, une phrase par livrable — le reviewer s'appuie dessus pour valider sans relire le code [+2]
- Mentionner les vérifications intermédiaires (ex: GET avant PUT) dans le commentaire de livraison pour tracer la démarche complète [+1]

- Quand l'owner pose une question dans un commentaire de ticket (ex: "Ca pourrait pas être centralisé ?"), c'est une instruction de refactoring — implémenter directement [+1]

## Patterns de succès

- Livraison directe et complète sans retour en Todo ni blocage [+2]

## Anti-patterns

## Préférences owner

- Préfère la centralisation (DRY) plutôt que la répétition dans chaque fichier [+1]

## Métriques

- tickets_complétés [4]
- tickets_retournés_todo [0]
- builds_cassés [0]
- tickets_bloqués [0]
