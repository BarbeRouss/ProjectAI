# Lessons Learned

Patterns et erreurs à éviter, capturés après corrections.

---

## 2026-03-12

### Sprint créé sans User Story correspondante
**Contexte:** Feature "Prochaines tâches" ajoutée directement dans sprint.md sans être dans user-stories.md.
**Cause:** Workflow incomplet - on a sauté l'étape d'ajout aux specs.
**Leçon:** TOUJOURS ajouter une US dans `specs/user-stories.md` AVANT de créer un sprint. Le sprint référence les US, pas l'inverse.

### Tests InMemory ne détectent pas les migrations manquantes
**Contexte:** L'API refusait de démarrer avec PendingModelChangesWarning, mais les tests passaient.
**Cause:** Les tests d'intégration utilisent `UseEnvironment("Testing")` avec base InMemory qui ne vérifie pas les migrations.
**Leçon:** Toujours vérifier que les migrations sont à jour avant de démarrer Aspire. Commande: `dotnet ef migrations list`.

### Port 22222 occupé après arrêt brutal d'Aspire
**Contexte:** Aspire refuse de démarrer car le port 22222 est occupé.
**Cause:** Le processus DCP d'Aspire n'a pas été arrêté proprement.
**Leçon:** Tuer le processus manuellement: `netstat -ano | findstr :22222` puis `taskkill /PID <PID> /F`.

---

## 2026-03-15

### Toujours tester les commandes Docker/build en local avant de push en CI
**Contexte:** Multiples itérations (6+) pour débugger le deploy CI sans pouvoir voir les logs.
**Cause:** Les commandes Docker et dotnet publish n'ont pas été testées localement d'abord. Chaque fix nécessitait un push + 5 min d'attente.
**Leçon:** TOUJOURS tester les commandes de build en local avant de les mettre dans le CI. Si Docker n'est pas dispo localement, au minimum valider `dotnet restore`, `dotnet build`, `npm run build`, et vérifier l'existence des fichiers référencés.

### GHCR exige des noms d'images en minuscules
**Contexte:** `docker push ghcr.io/BarbeRouss/...` échouait silencieusement.
**Cause:** `github.repository_owner` peut contenir des majuscules. GHCR refuse les majuscules.
**Leçon:** Toujours passer le owner en minuscules : `echo "$OWNER" | tr '[:upper:]' '[:lower:]'`.

### Ne pas mettre Aspire.Hosting.AppHost dans un projet service
**Contexte:** `dotnet restore` échouait dans le Dockerfile de l'API.
**Cause:** `Aspire.Hosting.AppHost` nécessite le workload Aspire, non disponible dans l'image Docker SDK standard.
**Leçon:** Ce package appartient au AppHost uniquement. Les projets service utilisent les packages client (ex: `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL`).

### Next.js standalone output pour Docker
**Contexte:** Le Dockerfile frontend copiait `node_modules` + `.next` mais `npm start` échouait.
**Cause:** Sans `output: 'standalone'`, Next.js a besoin de plus de fichiers pour fonctionner.
**Leçon:** Toujours utiliser `output: 'standalone'` dans `next.config.ts` pour les déploiements Docker. Le Dockerfile copie `.next/standalone/` + `.next/static/`.

### Vérifier l'existence des fichiers/dossiers référencés dans un Dockerfile
**Contexte:** `COPY --from=build /app/public ./public` échouait car le dossier n'existait pas.
**Cause:** Le Dockerfile a été écrit en supposant l'existence d'un dossier `public/`.
**Leçon:** Toujours vérifier avec `ls` que les fichiers/dossiers existent avant de les référencer dans un Dockerfile.

---

## 2026-03-23

### Toujours valider le build CI après chaque push — itérer si échec

**Contexte:** Des builds CI échouaient sans qu'on s'en rende compte, causant des retours tardifs et des itérations coûteuses.
**Cause:** Le push était considéré comme "terminé" sans vérifier le résultat du workflow.
**Leçon:** Après chaque `git push`, TOUJOURS vérifier que le build CI passe. Si échec, itérer jusqu'à ce que ça passe.

#### Procédure post-push obligatoire

```bash
# 1. Attendre que le workflow démarre (quelques secondes après le push)
#    Lister les runs récents pour trouver celui déclenché par notre push
gh run list --repo BarbeRouss/HouseFlow --branch <branch-name> --limit 3

# 2. Surveiller le run en cours (attente bloquante jusqu'à complétion)
gh run watch <run-id> --repo BarbeRouss/HouseFlow

# 3. Si le run échoue, consulter les logs pour identifier l'erreur
#    --failed filtre uniquement les étapes en échec (évite le bruit)
gh run view <run-id> --repo BarbeRouss/HouseFlow --log-failed

# 4. Si besoin de plus de contexte, voir les logs complets d'un job spécifique
gh run view <run-id> --repo BarbeRouss/HouseFlow --log

# 5. Corriger, commit, push, et recommencer à l'étape 1
```

#### Commandes GH utiles pour le debug CI

```bash
# Lister les 5 derniers runs (tous workflows)
gh run list --repo BarbeRouss/HouseFlow --limit 5

# Lister les runs d'un workflow spécifique
gh run list --repo BarbeRouss/HouseFlow --workflow "PR Checks" --limit 5
gh run list --repo BarbeRouss/HouseFlow --workflow "Deploy" --limit 5

# Voir le résumé d'un run (jobs, statuts, durées)
gh run view <run-id> --repo BarbeRouss/HouseFlow

# Voir uniquement les logs des étapes échouées (LE PLUS UTILE)
gh run view <run-id> --repo BarbeRouss/HouseFlow --log-failed

# Voir les logs complets (verbose, beaucoup de sortie)
gh run view <run-id> --repo BarbeRouss/HouseFlow --log

# Relancer un run échoué sans re-push
gh run rerun <run-id> --repo BarbeRouss/HouseFlow

# Relancer uniquement les jobs échoués
gh run rerun <run-id> --repo BarbeRouss/HouseFlow --failed
```

#### Pattern d'itération

1. `git push` → `gh run list` → noter le `<run-id>`
2. `gh run watch <run-id>` → attendre la fin
3. Si **success** → terminé
4. Si **failure** → `gh run view <run-id> --log-failed` → lire l'erreur
5. Corriger le code → commit → push → retour à l'étape 1
6. Répéter jusqu'à ce que le build soit vert

---

## Template

### [Titre court du problème]
**Contexte:** Qu'est-ce qui s'est passé ?
**Cause:** Pourquoi c'est arrivé ?
**Leçon:** Comment éviter à l'avenir ?
