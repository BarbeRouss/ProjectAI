# Plan d'Architecture Technique - Application de Suivi d'Entretien de Maison

## Objectif

Ce document propose une architecture technique complète pour le MVP de l'application de suivi d'entretien de maison, basée sur le cahier des charges fonctionnel existant. L'objectif est de définir les technologies, frameworks et services qui permettront de développer une application web moderne, scalable et maintenable.

## User Review Required

> [!IMPORTANT]
> **Choix de stack technique validés**
> - Backend : **ASP.NET Core 8 Web API**
> - Frontend : **En cours de discussion (Blazor vs React)**
> - BDD : PostgreSQL avec **Entity Framework Core**
> - Auth : **ASP.NET Core Identity** (Internal)
> - Paiements : **Stripe.NET SDK**
> - Hébergement : **Azure App Service / Railway**

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
- Application web full-stack moderne avec Server Components
- SSR (Server-Side Rendering) pour de meilleures performances et SEO
- Routing intégré et API routes
- TypeScript natif pour la robustesse du code
- Écosystème React riche pour les composants UI
- Déploiement simple sur Vercel

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

**Framework**: **ASP.NET Core 8 Web API**

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

**Structure du projet**:
```
/src
  /ProjectAI.API      # Projet Web API principal
  /ProjectAI.Data     # Context EF Core, Migrations, Modèles de BDD
  /ProjectAI.Core     # Logique métier, Interfaces, Services
  /ProjectAI.Shared   # DTOs partagés entre Frontend et Backend
```

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
- **Neon** (PostgreSQL serverless, tier gratuit généreux) ou
- **Supabase** (PostgreSQL + auth/storage, tier gratuit) ou
- **Vercel Postgres** (intégration native avec Vercel)

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

**Solution**: **Resend** ou **SendGrid**

**Resend** (Recommandé):
- API moderne et simple
- 3,000 emails/mois gratuits
- Templates React/JSX natifs
- Excellent pour Next.js

**Types d'emails**:
1. **Emails d'authentification** (vérification, reset password)
2. **Emails d'invitation** (collaborateurs, locataires)
3. **Rappels d'entretien** (automatiques selon périodicité)

**Architecture du système de rappels**:
- **Cron Job** quotidien (Vercel Cron ou service externe)
- Vérification des entretiens à venir dans les N prochains jours
- Envoi automatique d'emails de rappel
- Tracking des emails envoyés pour éviter les doublons

**Job scheduler**:
- **Vercel Cron** (gratuit, intégré) ou
- **Trigger.dev** (pour jobs plus complexes)

---

### Infrastructure et Déploiement

#### Hébergement

**Solution**: **Vercel** (Recommandé pour Next.js)

**Justification**:
- Optimisé pour Next.js
- Déploiement automatique depuis GitHub
- CDN global
- Preview deployments pour chaque PR
- SSL automatique
- Tier gratuit généreux

**Alternative**: **Railway** ou **Fly.io** (plus flexible mais configuration manuelle)

#### CI/CD

**GitHub Actions**:
- Linting et tests automatiques sur PR
- Déploiement automatique sur Vercel

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

**Mesures de sécurité**:
- **HTTPS** obligatoire (Vercel par défaut)
- **CORS** configuré strictement pour les API
- **Rate limiting** sur les endpoints sensibles (via middleware ou Vercel Edge Config)
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

### Monitoring

**Backend (C#)**:
- **Azure Application Insights**: Si hébergement sur Azure. Offre une vision complète des requêtes, exceptions et performances.
- **Serilog**: Pour le logging structuré. Permet d'envoyer les logs vers diverses destinations (Azure, Seq, ou fichiers).

**Frontend**:
- **Application Insights JS SDK** (si Azure) ou **Sentry** (si multi-cloud/Next.js) pour le tracking des erreurs client.

---

### Environnements & CI/CD

**Environnements**:
1. **Development**: Environnement de développement local (Docker Desktop / WSL).
2. **Staging**: Environnement de test identique à la production (ex: `staging.api.projectai.com`).
3. **Production**: Environnement final accessible aux utilisateurs.

**Workflow CI/CD**:
- **GitHub Actions**:
  - Build & Test sur chaque Pull Request.
  - Déploiement automatique vers Staging lors du merge sur `develop` (ou branche de staging).
  - Déploiement vers Production lors du merge sur `main` (avec approbation manuelle optionnelle).

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
