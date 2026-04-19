# Memory — code-janitor

## Leçons apprises

- **Vérifier l'état réel du code en début de run** : les sessions précédentes peuvent avoir partiellement appliqué des changements. Toujours grep avant d'éditer pour éviter de dupliquer un fix déjà fait.
- **Clés de localisation** : aucune construction dynamique de clés dans ce projet. Toutes les clés sont des string literals — grep sur `L["key"]` et `.Get("key")` est suffisant pour confirmer qu'une clé est orpheline.
- **Compte des fichiers .cs** : utiliser `find . -name "*.cs" | grep -v "/obj/" | grep -v "/bin/" | wc -l` — le rapport indiquait 55, la réalité est 46.
- **MSB3021** : erreur de build courante quand l'app est en cours d'exécution (DLL verrouillée). Ce n'est pas une erreur de compilation — vérifier `grep "warning CS|error CS"` séparément.
- **Dead properties** : une propriété initialisée (en constructor) mais jamais lue est dead — safe à supprimer (run 9).
- **Cleanup catches** : les `catch {}` dans `Dispose` et `DisposeAsync` sont best-effort cleanup (non-blocking) — doivent être documentées.
- **All catch blocks are now documented** : grep "catch {}" finds 0 results — all catches across the codebase are properly documented (run 10).
- **Project health plateau** : après run 11, codebase stabilisé à 98%, aucune nouvelle issue détectée. Les only remaining issues (#50, #63) requièrent des décisions architecturales hors du scope code-janitor.

## Patterns de succès

- Documenter les `catch {}` avec `/* comment */` est accepté (runs 6, 7, 9, 10).
- Supprimer les dead fields Blazor (run 7) — safe si grep confirme zéro lecture.
- Supprimer les clés JSON de localisation orphelines (run 8) — safe car aucune construction dynamique.
- Supprimer les dead properties des classes internes (run 9) — safe si grep confirme zéro lecture.

## Anti-patterns

- Ne pas éditer un fichier sans avoir d'abord grep pour confirmer l'état actuel (sessions multiples).
- Ne pas se fier au rapport de santé pour l'état du code : il peut être en retard d'un run.

## Préférences owner

- Tickets créés via PowerShell `Invoke-RestMethod` (UTF-8 safe sur Windows) — pas `curl`.
- Priorités valides : `Idea`, `NiceToHave`, `Required`, `Critical`.

## Métriques

- nettoyages_effectués [17]
- fichiers_supprimés [0]
- dépendances_nettoyées [1]
- tickets_créés [5]
