# Plan d'Architecture Technique - Application de Suivi d'Entretien de Maison

## Objectif

Ce document propose une architecture technique complète pour le MVP de l'application de suivi d'entretien de maison, basée sur le cahier des charges fonctionnel existant. L'objectif est de définir les technologies, frameworks et services qui permettront de développer une application web moderne, scalable et maintenable.

## User Review Required

> [!IMPORTANT]
> **Choix de stack technique validés**
> - Backend : **ASP.NET Core 10 Web API**
> - Orchestration & Observabilité : **.NET Aspire**
> - Conteneurisation : **Docker**
> - Frontend : **Next.js (React)** - Choix final.
- Design : **Consumer Moderne** - Choix final.
> - BDD : PostgreSQL avec **Entity Framework Core**
> - Auth : **ASP.NET Core Identity** (Internal)
> - Paiements : **Stripe.NET SDK**
> - Hébergement : **Azure Container Apps (via Aspire)** or Any Docker Proxy
> 
> Ces choix sont basés sur les besoins du MVP et peuvent être ajustés selon vos préférences ou contraintes.

> [!WARNING]
> **Implications des choix techniques**
> - L'hébergement sur Vercel + base de données externalisée implique des coûts mensuels
> - Le système d'emails nécessite un service tiers (coût additionnel)
> - Le système de paiement Stripe prend une commission sur chaque transaction

## Proposed Changes

### Frontend

#### Stack Technique Proposée

**Framework**: **Next.js 14+** (React avec App Router)

**Justification**:
- Choix du client pour apprendre le Frontend (JS/React).
- Déploiement via conteneur Docker sur **Azure Container Apps**.
- Intégration dans l'orchestration **.NET Aspire**.
- TypeScript pour la robustesse du code.

**Style de Design**: **Consumer Moderne** (Premium, vibrant, micro-animations).

**UI/UX**:
- **Tailwind CSS** pour le styling responsive
- **Shadcn/ui** pour les composants UI réutilisables et accessibles
- **Lucide React** pour les icônes
- **React Hook Form** + **Zod** pour la validation des formulaires

**State Management**:
- **React Query (TanStack Query)** pour la gestion du cache et des requêtes API
- **Zustand** ou **Context API** pour l'état global de l'application

**Authentification Frontend**:
- **NextAuth.js v5** (Auth.js) pour la gestion de sessions

---

### Backend & API

#### Architecture API

**Framework**: **ASP.NET Core 10 Web API**

**Justification**:
- Expertise du propriétaire en .NET facilitate la maintenance et la vérification du code.
- Performance élevée et maturité de l'écosystème.
- Séparation claire des responsabilités (Decoupled Architecture).
- Idéal pour une API RESTful robuste.

**ORM**:
- **Entity Framework Core (EF Core)**
- Approche Code-First ou Database-First au choix.
- Support natif de PostgreSQL via Npgsql.
- Fortement typé et intégré à l'écosystème .NET.

**Authentification / Autorisation**:
- **ASP.NET Core Identity** ou gestion d'IdentityServer/Duende.
- Support des JWT (JSON Web Tokens) pour la communication avec le frontend.
- Policies et Roles-based Authorization correspondant à la matrice de permissions.

**Architecture Backend : Onion Architecture**

L'application backend suivra les principes de l'**Onion Architecture** (également appelée Clean Architecture), qui garantit:
- **Indépendance des frameworks** : La logique métier ne dépend pas des détails d'infrastructure
- **Testabilité** : Chaque couche peut être testée indépendamment
- **Maintenabilité** : Séparation claire des responsabilités
- **Flexibilité** : Les dépendances pointent toujours vers le centre (Core)

**Structure du projet** :
```
/src
  /ProjectAI.Core                # Couche centrale - Entités et interfaces du domaine
    /Entities                    # Entités métier (House, Device, MaintenanceType, etc.)
    /Enums                       # Énumérations (AccessRole, Permission, etc.)
    /ValueObjects                # Objets valeur immuables
    /Interfaces                  # Interfaces des repositories et services
    
  /ProjectAI.Application         # Couche application - Logique métier et use cases
    /Services                    # Services métier (orchestration)
    /DTOs                        # Data Transfer Objects
    /Validators                  # FluentValidation validators
    /Mappings                    # AutoMapper profiles (DTO ↔ Core Entities)
    
  /ProjectAI.Infrastructure      # Couche infrastructure - Implémentations concrètes
    /Persistence                 # EF Core DbContext, Configurations, Migrations
    /Repositories                # Implémentations des repositories
    /Identity                    # ASP.NET Core Identity configuration
    /Services                    # Services externes (Email, Storage, etc.)
    /Mappings                    # Mappings EF (Core Entities ↔ DB Entities si nécessaire)
    
  /ProjectAI.API                 # Couche présentation - API Web
    /Controllers                 # Contrôleurs API REST
    /Middleware                  # Middlewares custom
    /Filters                     # Filtres d'action
    Program.cs                   # Point d'entrée, configuration DI
```

**Flux de dépendances (Onion)** :
```
API → Application → Core
  ↓
Infrastructure → Application (via interfaces définies dans Core)
```

**Note sur les mappings** :
- **Application/Mappings** : Conversion entre DTOs (API) et entités Core
- **Infrastructure/Mappings** : Conversion entre entités Core et entités de persistance EF Core (si vous souhaitez un découplage total entre le Core et EF Core)

---

### Base de Données

#### Choix de BDD

**Base principale**: **PostgreSQL**

**Justification**:
- Base relationnelle robuste adaptée à la hiérarchie des données
- Support natif de JSON pour données flexibles
- Contraintes d'intégrité référentielle
- Excellentes performances
- Support des transactions
- Gratuit et open-source

**Schema Principal**:

```prisma
model User {
  id            String   @id @default(cuid())
  email         String   @unique
  name          String?
  createdAt     DateTime @default(now())
  
  ownedOrganizations Organization[] @relation("OrganizationOwner")
  houses        HouseAccess[]
}

model Organization {
  id            String   @id @default(cuid())
  name          String
  ownerId       String
  owner         User     @relation("OrganizationOwner", fields: [ownerId], references: [id])
  
  houses        House[]
  subscription  Subscription?
}

model House {
  id              String   @id @default(cuid())
  name            String
  address         String?
  organizationId  String?
  organization    Organization? @relation(fields: [organizationId], references: [id])
  
  devices         Device[]
  accesses        HouseAccess[]
}

model HouseAccess {
  id          String   @id @default(cuid())
  houseId     String
  house       House    @relation(fields: [houseId], references: [id])
  userId      String
  user        User     @relation(fields: [userId], references: [id])
  
  role        AccessRole  // OWNER, COLLABORATOR, TENANT
  permission  Permission  // READ, READ_WRITE
}

model Device {
  id              String   @id @default(cuid())
  name            String
  type            DeviceType
  installDate     DateTime?
  metadata        Json?    // marque, modèle, etc.
  houseId         String
  house           House    @relation(fields: [houseId], references: [id])
  
  maintenanceTypes MaintenanceType[]
}

model MaintenanceType {
  id              String   @id @default(cuid())
  name            String
  periodicity     String   // ANNUAL, SEMESTRIAL, QUARTERLY, CUSTOM
  customDays      Int?     // Pour périodicité custom
  reminderEnabled Boolean  @default(true)
  reminderDaysBefore Int   @default(30)
  deviceId        String
  device          Device   @relation(fields: [deviceId], references: [id])
  
  instances       MaintenanceInstance[]
}

model MaintenanceInstance {
  id                  String   @id @default(cuid())
  date                DateTime
  cost                Decimal?
  provider            String?
  notes               String?
  status              MaintenanceStatus // COMPLETED, PLANNED, OVERDUE
  maintenanceTypeId   String
  maintenanceType     MaintenanceType @relation(fields: [maintenanceTypeId], references: [id])
}

model Subscription {
  id              String   @id @default(cuid())
  organizationId  String   @unique
  organization    Organization @relation(fields: [organizationId], references: [id])
  stripeCustomerId String  @unique
  stripePriceId   String
  status          SubscriptionStatus
  currentPeriodEnd DateTime
}
```

**Hébergement BDD**:
- **Azure Database for PostgreSQL** (Production).
- **Conteneur PostgreSQL** orchestré par .NET Aspire (Développement local et potentiellement Staging).

---

### Authentification et Autorisation

#### Système d'Auth

**Solution**: **NextAuth.js v5 (Auth.js)**

**Providers**:
- Email/Password (credentials provider)
- OAuth optionnel (Google, GitHub) pour faciliter l'inscription

**Session Management**:
- JWT tokens pour les sessions
- Stockage des sessions en base de données pour révocation

**Authorization**:
- Middleware Next.js pour protéger les routes
- Vérification des permissions à chaque requête API basée sur `HouseAccess`
- Helper functions pour vérifier les permissions selon la matrice définie

**Structure des permissions**:
```typescript
enum AccessRole {
  OWNER
  COLLABORATOR
  TENANT
}

enum Permission {
  READ
  READ_WRITE
}

// Fonction de vérification
async function checkPermission(
  userId: string,
  houseId: string,
  requiredPermission: Permission
): Promise<boolean>
```

---

### Système de Notifications (Emails)

#### Service Email

**Solution**: **Azure App Service** (SMTP) ou **SendGrid** avec intégration .NET.

**Gestion du Multilingue (Communications)**
- **MVP** : Support **Français et Anglais**.
- **Localisation Backend** : Utilisation de `IStringLocalizer` et de fichiers de ressources `.resx` pour les chaînes simples.
- **Templates Email** : Templates localisés (ex: `Welcome.fr.html`, `Welcome.en.html`) ou moteur de template supportant la localisation (Razor/Liquid).
- **Préférence Utilisateur** : La langue de communication est stockée dans la table `User` et envoyée dans les headers ou résolue côté backend pour les notifications asynchrones.

**Types d'emails**:
1. **Emails d'authentification** (vérification, reset password)
2. **Emails d'invitation** (collaborateurs, locataires)
3. **Rappels d'entretien** (automatiques selon périodicité)

**Architecture du système de rappels**:
- **Cron Job** quotidien (Vercel Cron ou service externe)
- Vérification des entretiens à venir dans les N prochains jours
- Envoi automatique d'emails de rappel
- Tracking des emails envoyés pour éviter les doublons

- **Job Scheduler** : Background Service .NET, Hangfire, ou Azure Container Apps Jobs orchestrés par Aspire.

---

#### Hébergement & Déploiement

**Solution**: **Azure Container Apps** (via .NET Aspire)

**Justification**:
- Optimisé pour .NET Aspire via `azd`.
- Déploiement automatique depuis GitHub Actions.
- Ingress et SSL gérés par Azure.
- Scalabilité de 0 à N (coût réduit au repos).

#### CI/CD

**Azure Developer CLI (`azd`) + GitHub Actions**:
- Provisionnement automatique des ressources Azure (Bicep/Terraform généré par Aspire).
- Pipeline de build et déploiement intégré.

#### Environnements

- **Production** (branche `main`)
- **Staging** (branche `develop`)
- **Preview** (chaque PR)

#### Variables d'environnement

```env
# Database
DATABASE_URL=

# Auth
NEXTAUTH_SECRET=
NEXTAUTH_URL=

# Email
RESEND_API_KEY=

# Stripe
STRIPE_SECRET_KEY=
STRIPE_WEBHOOK_SECRET=
NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=

# App
NEXT_PUBLIC_APP_URL=
```

---

### Système de Paiement (Abonnements)

#### Solution de paiement

**Stripe**

**Justification**:
- Leader du marché des paiements en ligne
- Gestion native des abonnements récurrents
- Support des webhooks pour synchronisation
- Excellent SDK JavaScript/TypeScript
- Portal client pour gérer l'abonnement
- Support SEPA, cartes bancaires européennes

**Architecture**:

1. **Produits Stripe**:
   - Free Tier (sans abonnement Stripe)
   - Premium Plan (abonnement mensuel récurrent)

2. **Flow d'abonnement**:
   - Utilisateur clique "Upgrade to Premium"
   - Création de Stripe Checkout Session
   - Redirection vers Stripe Checkout
   - Paiement et retour sur l'app
   - Webhook Stripe confirme le paiement
   - Activation de l'Organisation dans la BDD

3. **Webhooks Stripe** (route `/api/webhooks/stripe`):
   - `checkout.session.completed` → Créer l'Organisation et Subscription
   - `invoice.paid` → Renouvellement réussi
   - `invoice.payment_failed` → Suspension de l'Organisation
   - `customer.subscription.deleted` → Suppression de l'abonnement

4. **Customer Portal**:
   - Stripe Customer Portal pour que les utilisateurs puissent:
     - Modifier leur carte bancaire
     - Voir leurs factures
     - Annuler leur abonnement

**Gestion des limitations**:
- Check à chaque création de maison si l'utilisateur a un abonnement actif
- Si gratuit → maximum 1 maison
- Si premium → maisons illimitées

---

### Sécurité et Conformité

#### Sécurité

**Sécurité**:
- **Azure Key Vault** pour la gestion des secrets en production.
- **HTTPS** obligatoire via l'Ingress d'Azure Container Apps.
- **Validation des inputs** avec Zod sur toutes les requêtes API
- **SQL Injection** prévenue par Prisma ORM
- **XSS** prévenu par React (escape automatique)
- **CSRF** tokens via NextAuth

**Authentification**:
- Passwords hashés avec **bcrypt** ou **argon2**
- Tokens JWT signés
- Sessions expirables

#### RGPD

**Conformité RGPD**:
- **Consentement explicite** pour les emails marketing (opt-in)
- **Droit à l'oubli** : endpoint pour supprimer toutes les données utilisateur
- **Export de données** : endpoint pour exporter les données en JSON
- **Politique de confidentialité** affichée
- **Cookies** : banner de consentement si tracking externe

**Données personnelles stockées**:
- Email, nom (minimum)
- Données de maisons/appareils (propriétaire des données)
- Logs d'accès (minimisés, anonymisés après 90 jours)

#### Backup

- Backups automatiques de la base de données (fournis par Neon/Supabase/Vercel)
- Rétention 7 jours minimum

---

### Hébergement & Orchestration

**Orchestration Locale : .NET Aspire**
- Utilisation de **.NET Aspire** pour orchestrer le backend, le frontend et les ressources (Postgres, Redis, webhooks).
- Simplifie la découverte de services et la gestion des chaînes de connexion.
- Fournit un dashboard de monitoring local (Traces, Logs, Metrics) via OpenTelemetry.

**Conteneurisation : Docker**
- Toute la stack est conteneurisée.
- Indépendance vis-à-vis du cloud provider : peut être déployé sur n'importe quel service supportant Docker (VPS, Railway, Fly.io, Azure).

**Hébergement Cloud : Azure Container Apps**
- Cible privilégiée pour .NET Aspire.
- Scalabilité à zéro (coût réduit au repos).
- Gestion simplifiée des certificats et de l'ingress.

---

### Monitoring

**Observabilité native avec .NET Aspire**:
- **OpenTelemetry** intégré par défaut.
- **Azure Application Insights** : Destination pour les données télémétriques exportées par Aspire.
- **Dashboard Aspire** pour le debug local.

---

### Environnements & CI/CD

**Docker-Compose / Aspire AppHost**:
- Permet de lancer `dotnet run` sur l'AppHost pour démarrer toute la stack localement.

---

## Verification Plan

### Validation Technique

1. **Review du choix de stack** avec le client
2. **Validation des coûts estimés** pour l'infrastructure
3. **Confirmation de la conformité RGPD** selon le marché cible

### Prochaines Étapes

Après validation de ce plan d'architecture:

1. **Setup du projet**:
   - Initialisation Next.js + TypeScript
   - Configuration Prisma + PostgreSQL
   - Setup NextAuth.js

2. **Développement des features MVP**:
   - Authentification et gestion utilisateurs
   - CRUD Maisons/Appareils/Entretiens
   - Système de permissions
   - Intégration Stripe
   - Système de rappels emails

3. **Tests et déploiement**:
   - Tests unitaires et d'intégration
   - Déploiement staging
   - Tests utilisateurs
   - Production

---

**Estimation des coûts mensuels (approximatifs)**:

| Service | Tier Gratuit | Coût estimé (croissance) |
|---------|--------------|--------------------------|
| Vercel | Oui (hobby) | 0€ → 20€/mois (Pro) |
| Neon/Supabase | Oui | 0€ → 25€/mois |
| Resend | 3k emails/mois | 0€ → 20€/mois |
| Stripe | Commission | 1,5% + 0,25€ par transaction |
| Sentry | Limité | 0€ → 26€/mois |

**Total estimé MVP**: 0€/mois (tier gratuits suffisants pour démarrer)  
**Total croissance**: ~90-120€/mois pour 1000+ utilisateurs actifs
