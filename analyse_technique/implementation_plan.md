# Plan d'Architecture Technique - Application de Suivi d'Entretien de Maison

## Objectif

Ce document propose une architecture technique complète pour le MVP de l'application de suivi d'entretien de maison, basée sur le cahier des charges fonctionnel existant. L'objectif est de définir les technologies, frameworks et services qui permettront de développer une application web moderne, scalable et maintenable.

## User Review Required

> [!IMPORTANT]
> **Choix de stack technique validés**
> - Backend : **ASP.NET Core 8 Web API**
> - Orchestration & Observabilité : **.NET Aspire**
> - Conteneurisation : **Docker**
> - Frontend : **En cours de discussion (Blazor vs React)**
> - BDD : PostgreSQL avec **Entity Framework Core**
> - Auth : **ASP.NET Core Identity** (Internal)
> - Paiements : **Stripe.NET SDK**
> - Hébergement : **Azure Container Apps (via Aspire)** or Any Docker Proxy

> [!WARNING]
> **Implications des choix techniques**
> - L'hébergement sur Azure Container Apps implique des coûts basés sur la consommation (scalability to zero).
> - Le système d'emails nécessite un service tiers (coût additionnel).
> - Le système de paiement Stripe prend une commission sur chaque transaction.

## Proposed Changes

### Frontend

#### Stack Technique Proposée

**Framework**: **Next.js 14+** (React avec App Router) or **Blazor**

**Justification**:
- Déploiement via conteneur Docker sur **Azure Container Apps**.
- Intégration dans l'orchestration **.NET Aspire**.
- TypeScript (Next.js) ou C# (Blazor) selon le choix final.

**UI/UX**:
- **Tailwind CSS** pour le styling responsive
- **Shadcn/ui** pour les composants UI réutilisables et accessibles
- **Lucide React** pour les icônes
- **React Hook Form** + **Zod** pour la validation des formulaires

**State Management**:
- **React Query (TanStack Query)** pour la gestion du cache et des requêtes API
- **Zustand** ou **Context API** pour l'état global de l'application

**Authentification Frontend**:
- **NextAuth.js v5** (si Next.js) ou intégration native Identity (si Blazor).

---

### Backend & API

#### Architecture API

**Framework**: **ASP.NET Core 8 Web API**

**Justification**:
- Expertise du propriétaire en .NET facilite la maintenance et la vérification du code.
- Performance élevée et maturité de l'écosystème.
- Séparation claire des responsabilités (Decoupled Architecture).
- Idéal pour une API RESTful robuste.

**ORM**:
- **Entity Framework Core (EF Core)**
- Approche Code-First ou Database-First au choix.
- Support natif de PostgreSQL via Npgsql.
- Fortement typé et intégré à l'écosystème .NET.

**Authentification / Autorisation**:
- **ASP.NET Core Identity**.
- Support des JWT (JSON Web Tokens) pour la communication avec le frontend.
- Policies et Roles-based Authorization correspondant à la matrice de permissions.

**Structure du projet**:
```
/src
  /ProjectAI.AppHost  # Projet Aspire AppHost
  /ProjectAI.ServiceDefaults # Defaults Aspire
  /ProjectAI.API      # Projet Web API principal
  /ProjectAI.Data     # Context EF Core, Migrations, Modèles de BDD
  /ProjectAI.Core     # Logique métier, Interfaces, Services
  /ProjectAI.Shared   # DTOs partagés
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

**Hébergement BDD**:
- **Azure Database for PostgreSQL** (Production).
- **Conteneur PostgreSQL** orchestré par .NET Aspire (Développement local et potentiellement Staging).

---

### Authentification et Autorisation

#### Système d'Auth

**Solution**: **ASP.NET Core Identity**

**Session Management**:
- JWT tokens pour les sessions API.

**Authorization**:
- Vérification des permissions à chaque requête API basée sur `HouseAccess`.
- Policies-based authorization.

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
- Job quotidien vérifiant les entretiens à venir.
- Envoi automatique d'emails de rappel.
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

---

### Sécurité et Conformité

#### Sécurité

**Sécurité**:
- **Azure Key Vault** pour la gestion des secrets en production.
- **HTTPS** obligatoire via l'Ingress d'Azure Container Apps.
- **Validation des inputs** sur toutes les requêtes API.
- **SQL Injection** prévenue par EF Core.
- **XSS** prévenu par Blazor/React.
- **CSRF** protection activée.

#### Monitoring

**Observabilité native avec .NET Aspire**:
- **OpenTelemetry** intégré par défaut.
- **Azure Application Insights** : Destination pour les données télémétriques.
- **Dashboard Aspire** pour le debug local.

---

## Verification Plan

### Validation Technique

1. **Review du choix de stack** avec le client
2. **Validation des coûts estimés** pour l'infrastructure
3. **Confirmation de la conformité RGPD** selon le marché cible

---

**Estimation des coûts mensuels (approximatifs)**:

| Service | Tier Gratuit | Coût estimé (croissance) |
|---------|--------------|--------------------------|
| Azure Container Apps | Oui (Consommation) | 0€ → Variable |
| Database | Conteneur (Gratuit) | 0€ → 25€/mois (Azure DB) |
| SendGrid/SMTP | Oui | 0€ → 20€/mois |
| Stripe | Commission | 1,5% + 0,25€ par transaction |

**Total estimé MVP**: ~0€/mois (crédits gratuits Azure ou tier consommation).
