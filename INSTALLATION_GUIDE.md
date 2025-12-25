# Guide d'Installation

## Prérequis

1. **.NET 10 SDK** - https://dotnet.microsoft.com/download
2. **Node.js 20+** - https://nodejs.org/
3. **PostgreSQL 16** (optionnel, Aspire le démarre automatiquement)

Vérifier:
```bash
dotnet --version  # 10.0.x
node --version    # v20.x.x
npm --version     # v10.x.x
```

## Installation

### 1. Backend (.NET)

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Tests
dotnet test
```

### 2. Frontend (Next.js)

#### Option A: Script automatique

**Windows:**
```bash
cd src\HouseFlow.Frontend
INSTALL.bat
```

**Linux/Mac:**
```bash
cd src/HouseFlow.Frontend
chmod +x install.sh
./install.sh
```

#### Option B: Manuel

```bash
cd src/HouseFlow.Frontend
npm install                 # 2-3 min
npx playwright install      # 3-5 min
```

## Démarrage

### Via Aspire (Recommandé)

```bash
dotnet run --project src/HouseFlow.AppHost
```

Démarre automatiquement:
- PostgreSQL (port 5432)
- HouseFlow.API (port 5203)
- HouseFlow.Frontend (port 3000)

**Accès:**
- Frontend: http://localhost:3000
- API: http://localhost:5203
- Aspire Dashboard: http://localhost:15000

### Mode Développement Séparé

**Backend:**
```bash
# Terminal 1: API
cd src/HouseFlow.API
dotnet run
```

**Frontend:**
```bash
# Terminal 2: Frontend
cd src/HouseFlow.Frontend
npm run dev
```

## Tests

**Backend:**
```bash
dotnet test
```

**Frontend E2E:**
```bash
cd src/HouseFlow.Frontend
npm test              # Tous les tests
npm run test:ui       # Mode interactif
npm run test:debug    # Mode debug
```

## Dépannage

### Port déjà utilisé

**Windows:**
```bash
netstat -ano | findstr :3000
taskkill /PID <PID> /F
```

**Linux/Mac:**
```bash
lsof -ti:3000 | xargs kill -9
```

### npm install échoue

```bash
npm cache clean --force
rm -rf node_modules package-lock.json
npm install
```

### Build errors

```bash
dotnet clean
dotnet restore
dotnet build
```

## Structure Post-Installation

```
src/HouseFlow.Frontend/
├── node_modules/     ← Créé par npm install (~800 MB)
├── .next/           ← Créé par npm run dev
├── package.json     ← Déjà présent
└── ...
```

## Vérification

```bash
# Backend
dotnet build        # Devrait réussir

# Frontend
cd src/HouseFlow.Frontend
npx next --version          # 15.1.3
npx playwright --version    # 1.49.1
```
