# DevContainer pour Claude Code Isolé

Ce devcontainer permet d'exécuter Claude CLI dans un environnement Docker isolé avec accès limité au dossier du projet uniquement.

## Prérequis

- Docker Desktop installé et en cours d'exécution
- Visual Studio Code avec l'extension "Dev Containers" installée
- Une clé API Anthropic (variable d'environnement `ANTHROPIC_API_KEY`)

## Configuration de la clé API

Avant de lancer le devcontainer, assurez-vous que votre clé API est configurée :

### Windows (PowerShell)
```powershell
$env:ANTHROPIC_API_KEY="votre_clé_api"
```

### Linux/macOS
```bash
export ANTHROPIC_API_KEY="votre_clé_api"
```

Ou ajoutez-la de manière permanente dans votre profil shell (~/.bashrc, ~/.zshrc, etc.).

## Utilisation

1. Ouvrez le projet dans VS Code
2. Appuyez sur `F1` et sélectionnez "Dev Containers: Reopen in Container"
3. Attendez que le container se construise (première fois seulement)
4. Une fois dans le container, ouvrez un terminal

## Lancer Claude

Dans le terminal du devcontainer :

```bash
# Lancer Claude avec skip permissions
claude --dangerously-skip-permissions

# Ou avec d'autres options
claude --dangerously-skip-permissions --model sonnet
```

## Sécurité et Isolation

- **Accès limité** : Le container n'a accès qu'au dossier `/workspace` (votre projet)
- **Utilisateur non-root** : Exécution en tant qu'utilisateur `vscode`
- **Pas de privilèges supplémentaires** : Option `--security-opt=no-new-privileges`
- **Pas de montages système** : Aucun accès aux fichiers en dehors du workspace

## Fichiers créés

- **Dockerfile** : Définit l'image avec Node.js et Claude CLI
- **devcontainer.json** : Configuration du devcontainer avec restrictions de sécurité

## Dépannage

### Claude n'est pas trouvé
Si la commande `claude` n'est pas reconnue, réinstallez-la :
```bash
npm install -g @anthropic-ai/claude-code
```

### Problèmes de clé API
Vérifiez que la variable d'environnement est bien définie :
```bash
echo $ANTHROPIC_API_KEY
```

### Reconstruire le container
Si vous modifiez le Dockerfile :
1. `F1` > "Dev Containers: Rebuild Container"
2. Ou supprimez le container et relancez

## Notes importantes

- L'option `--dangerously-skip-permissions` désactive certaines vérifications de sécurité de Claude
- Utilisez cette option uniquement dans un environnement contrôlé comme ce devcontainer
- Le container est isolé et ne peut pas accéder à vos fichiers système
