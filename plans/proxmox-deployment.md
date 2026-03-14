# Plan : Déploiement Proxmox Self-Hosted

## Résumé des décisions

| Question | Décision |
|----------|----------|
| Infra | VM unique Debian 12 sur Proxmox |
| Orchestration | Docker Compose |
| Reverse proxy | Traefik v3 (SSL Let's Encrypt auto) |
| Accès | Exposé sur internet (domaine géré par l'utilisateur) |
| CI/CD | GitHub Actions → SSH deploy auto sur push main |
| Backups | PostgreSQL dumps quotidiens + rétention 7j |
| Monitoring | Aspire Dashboard (premier temps) |

---

## Architecture cible

```
Internet
  │
  ▼
Proxmox Host
└── VM Debian 12 (Docker)
    └── Docker Compose
        ├── traefik        :80/:443  (reverse proxy, SSL auto)
        ├── houseflow-api  :5203     (interne)
        ├── houseflow-web  :3000     (interne)
        ├── postgres       :5432     (interne)
        └── aspire-dashboard :18888  (optionnel, accès restreint)
```

Traefik gère le routage :
- `app.{domaine}` → frontend (Next.js)
- `api.{domaine}` → API (.NET)
- `dashboard.{domaine}` → Aspire Dashboard (BasicAuth)

---

## Étapes d'implémentation

### 1. Dockerfile API (.NET 10)

**Fichier :** `src/HouseFlow.API/Dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY *.sln ./
COPY src/HouseFlow.Core/*.csproj src/HouseFlow.Core/
COPY src/HouseFlow.Application/*.csproj src/HouseFlow.Application/
COPY src/HouseFlow.Infrastructure/*.csproj src/HouseFlow.Infrastructure/
COPY src/HouseFlow.API/*.csproj src/HouseFlow.API/
RUN dotnet restore src/HouseFlow.API/HouseFlow.API.csproj
COPY . .
RUN dotnet publish src/HouseFlow.API/HouseFlow.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "HouseFlow.API.dll"]
```

Multi-stage build. L'image runtime ne contient pas le SDK (~10x plus légère).

### 2. Dockerfile Frontend (Next.js 15)

**Fichier :** `src/HouseFlow.Frontend/Dockerfile`

```dockerfile
FROM node:22-alpine AS deps
WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production

FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
ENV NEXT_TELEMETRY_DISABLED=1
RUN npm run build

FROM node:22-alpine AS runtime
WORKDIR /app
ENV NODE_ENV=production
COPY --from=deps /app/node_modules ./node_modules
COPY --from=build /app/.next ./.next
COPY --from=build /app/public ./public
COPY --from=build /app/package.json ./
COPY --from=build /app/next.config.ts ./
COPY --from=build /app/src/messages ./src/messages
EXPOSE 3000
CMD ["npm", "start"]
```

### 3. Docker Compose Production

**Fichier :** `docker-compose.prod.yml`

```yaml
services:
  traefik:
    image: traefik:v3.2
    command:
      - --api.dashboard=false
      - --providers.docker=true
      - --providers.docker.exposedbydefault=false
      - --entrypoints.web.address=:80
      - --entrypoints.websecure.address=:443
      - --certificatesresolvers.letsencrypt.acme.httpchallenge=true
      - --certificatesresolvers.letsencrypt.acme.httpchallenge.entrypoint=web
      - --certificatesresolvers.letsencrypt.acme.email=${ACME_EMAIL}
      - --certificatesresolvers.letsencrypt.acme.storage=/letsencrypt/acme.json
      - --entrypoints.web.http.redirections.entrypoint.to=websecure
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock:ro
      - letsencrypt:/letsencrypt
    restart: unless-stopped

  api:
    build:
      context: .
      dockerfile: src/HouseFlow.API/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__houseflow=Host=postgres;Database=houseflow;Username=${DB_USER};Password=${DB_PASSWORD}
      - JWT__KEY=${JWT_KEY}
      - Jwt__Issuer=${JWT_ISSUER}
      - Jwt__Audience=${JWT_AUDIENCE}
      - CORS__Origins=${CORS_ORIGINS}
    labels:
      - traefik.enable=true
      - traefik.http.routers.api.rule=Host(`${API_HOST}`)
      - traefik.http.routers.api.entrypoints=websecure
      - traefik.http.routers.api.tls.certresolver=letsencrypt
      - traefik.http.services.api.loadbalancer.server.port=8080
    depends_on:
      postgres:
        condition: service_healthy
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/alive"]
      interval: 30s
      timeout: 5s
      retries: 3

  web:
    build:
      context: src/HouseFlow.Frontend
      dockerfile: Dockerfile
    environment:
      - NODE_ENV=production
      - NEXT_PUBLIC_API_URL=https://${API_HOST}
    labels:
      - traefik.enable=true
      - traefik.http.routers.web.rule=Host(`${APP_HOST}`)
      - traefik.http.routers.web.entrypoints=websecure
      - traefik.http.routers.web.tls.certresolver=letsencrypt
      - traefik.http.services.web.loadbalancer.server.port=3000
    depends_on:
      - api
    restart: unless-stopped

  postgres:
    image: postgres:16-alpine
    environment:
      - POSTGRES_DB=houseflow
      - POSTGRES_USER=${DB_USER}
      - POSTGRES_PASSWORD=${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${DB_USER}"]
      interval: 5s
      timeout: 5s
      retries: 5
    restart: unless-stopped

volumes:
  postgres_data:
  letsencrypt:
```

### 4. Fichier .env.example

**Fichier :** `.env.example`

```env
# Domain
APP_HOST=app.example.com
API_HOST=api.example.com
ACME_EMAIL=you@example.com
CORS_ORIGINS=https://app.example.com

# Database
DB_USER=houseflow
DB_PASSWORD=CHANGE_ME_STRONG_PASSWORD

# JWT (minimum 32 characters)
JWT_KEY=CHANGE_ME_MINIMUM_32_CHARS_SECRET_KEY
JWT_ISSUER=https://api.example.com
JWT_AUDIENCE=https://app.example.com
```

### 5. Adaptations du code existant

#### 5a. API Program.cs — Support Production sans Aspire

Le `else` block actuel utilise `builder.AddNpgsqlDbContext` (Aspire). En production Docker, on n'a pas Aspire. Il faut ajouter un chemin "Production" qui utilise une connection string standard, comme le mode CI.

```csharp
// Modifier le else block pour supporter Production sans Aspire
else if (builder.Environment.IsProduction())
{
    var connectionString = builder.Configuration.GetConnectionString("houseflow")
        ?? throw new InvalidOperationException("ConnectionStrings:houseflow not configured");
    builder.Services.AddDbContext<HouseFlowDbContext>(options =>
        options.UseNpgsql(connectionString, npgsqlOptions =>
            npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
}
else
{
    // Development: Aspire-managed
    builder.AddNpgsqlDbContext<HouseFlowDbContext>("houseflow", ...);
}
```

#### 5b. API Program.cs — CORS dynamique

Remplacer les origines CORS hardcodées par une variable d'environnement :

```csharp
var corsOrigins = Environment.GetEnvironmentVariable("CORS__Origins")?.Split(',')
    ?? new[] { "http://localhost:3000", "https://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

#### 5c. API Program.cs — Migrations automatiques en Production

Ajouter l'auto-migration pour Production (safe pour une app single-instance) :

```csharp
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    // Auto-migrate (safe for single-instance deployments)
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<HouseFlowDbContext>();
    dbContext.Database.Migrate();
}
```

Sans le seed admin (qui reste Development only).

#### 5d. Next.js — Variable d'environnement API URL

Le `next.config.ts` utilise déjà `services__api__*` avec fallback `localhost:5203`. Pour la prod, on passera `NEXT_PUBLIC_API_URL` via l'env Docker. Il faudra peut-être adapter le config pour le supporter.

### 6. Script de backup

**Fichier :** `scripts/backup.sh`

```bash
#!/bin/bash
# PostgreSQL daily backup with 7-day retention
BACKUP_DIR="/opt/houseflow/backups"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
RETENTION_DAYS=7

mkdir -p "$BACKUP_DIR"

# Dump via docker compose
docker compose -f /opt/houseflow/docker-compose.prod.yml exec -T postgres \
  pg_dump -U "$DB_USER" houseflow | gzip > "$BACKUP_DIR/houseflow_$TIMESTAMP.sql.gz"

# Cleanup old backups
find "$BACKUP_DIR" -name "*.sql.gz" -mtime +$RETENTION_DAYS -delete

echo "[$(date)] Backup completed: houseflow_$TIMESTAMP.sql.gz"
```

Cron : `0 3 * * * /opt/houseflow/scripts/backup.sh >> /var/log/houseflow-backup.log 2>&1`

### 7. CI/CD — GitHub Actions deploy.yml

**Stratégie :** SSH dans la VM, git pull, rebuild & restart.

```yaml
name: Deploy

on:
  push:
    branches: [main]

jobs:
  deploy:
    name: Deploy to Proxmox
    runs-on: ubuntu-latest
    timeout-minutes: 15

    steps:
      - name: Deploy via SSH
        uses: appleboy/ssh-action@v1
        with:
          host: ${{ secrets.DEPLOY_HOST }}
          username: ${{ secrets.DEPLOY_USER }}
          key: ${{ secrets.DEPLOY_SSH_KEY }}
          script: |
            cd /opt/houseflow
            git pull origin main
            docker compose -f docker-compose.prod.yml build --no-cache
            docker compose -f docker-compose.prod.yml up -d
            docker image prune -f
```

**Secrets GitHub nécessaires :**
- `DEPLOY_HOST` : IP publique ou DDNS
- `DEPLOY_USER` : utilisateur SSH
- `DEPLOY_SSH_KEY` : clé privée SSH

### 8. Documentation setup VM

**Fichier :** `docs/deployment.md`

Guide pour le setup initial de la VM :
1. Créer VM Debian 12 sur Proxmox (2 CPU, 4GB RAM, 40GB disk)
2. Installer Docker + Docker Compose
3. Cloner le repo dans `/opt/houseflow`
4. Copier `.env.example` → `.env` et configurer
5. `docker compose -f docker-compose.prod.yml up -d`
6. Configurer le cron backup
7. Ajouter les secrets GitHub pour le CI/CD

---

## Fichiers à créer/modifier

| Action | Fichier |
|--------|---------|
| Créer | `src/HouseFlow.API/Dockerfile` |
| Créer | `src/HouseFlow.Frontend/Dockerfile` |
| Créer | `docker-compose.prod.yml` |
| Créer | `.env.example` |
| Créer | `scripts/backup.sh` |
| Créer | `docs/deployment.md` |
| Créer | `src/HouseFlow.API/.dockerignore` |
| Créer | `src/HouseFlow.Frontend/.dockerignore` |
| Modifier | `src/HouseFlow.API/Program.cs` (Production DB + CORS + migrations) |
| Modifier | `src/HouseFlow.Frontend/next.config.ts` (si nécessaire pour API URL) |
| Modifier | `.github/workflows/deploy.yml` (SSH deploy) |
| Modifier | `specs/user-stories.md` (ajouter US-060) |

---

## Ordre d'exécution

1. **User Story** — Ajouter US-060 dans les specs
2. **Dockerfiles** — API + Frontend (testables localement)
3. **Adaptations code** — Program.cs (Production mode, CORS, migrations)
4. **Docker Compose prod** — Avec Traefik + .env
5. **CI/CD** — deploy.yml fonctionnel
6. **Backups** — Script + doc cron
7. **Documentation** — Guide setup VM
8. **Sprint** — Créer le sprint dans tasks/sprint.md
