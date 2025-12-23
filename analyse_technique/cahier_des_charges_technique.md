# Cahier des Charges - Application de Suivi d'Entretien de Maison

## 1. Présentation du Projet

### 1.1 Objectif
Développer une application web permettant aux utilisateurs de suivre et gérer les entretiens de leur(s) maison(s) et équipements (chaudière, poêle à bois, alarmes incendie, etc.).

### 1.2 Cible Utilisateurs
- **Propriétaires individuels** : gestion d'une seule propriété (offre gratuite)
- **Propriétaires multi-propriétés** : gestion de plusieurs maisons (offre payante)
- **Gestionnaires immobiliers** : gestion professionnelle de plusieurs propriétés (offre payante)
- **Locataires** : accès délégué en lecture ou lecture/écriture (gratuit)

---

## 2. Architecture de Données

### 2.1 Hiérarchie
```
Organisation (niveau payant)
    ↓
Maison(s)
    ↓
Appareil(s)
    ↓
Type d'Entretien
    ↓
Instance d'Entretien (réalisations)
```

### 2.2 Description des Entités

#### **Organisation**
- Niveau racine pour la gestion multi-propriétés
- **Accès** : Fonctionnalité payante (abonnement mensuel)
- Permet de créer plusieurs maisons

#### **Maison**
- Représente une propriété
- **Accès gratuit** : 1 maison
- **Accès payant** : 2+ maisons (via Organisation)
- Attributs :
  - Nom/adresse
  - Liste des collaborateurs avec niveaux de permissions

#### **Appareil**
- Équipement à entretenir dans une maison
- Attributs :
  - Nom personnalisé (défini par l'utilisateur)
  - Type (sélection dans une liste prédéfinie : chaudière, poêle à pellet, alarme incendie, etc. + **Autre**)
  - Date d'installation (optionnel)
  - Informations complémentaires (marque, modèle, etc.)

#### **Type d'Entretien**
- Définit les différents entretiens pour un appareil
- Exemples pour un poêle à pellet :
  - Entretien annuel
  - Ramonage
  - Contrôle technique
- Attributs :
  - Nom de l'entretien
  - Périodicité (annuel, semestriel, trimestriel, personnalisé, etc.)
  - Suggestion de périodicité légale (à terme, selon pays et type d'appareil) applicable à la périodicité
  - Rappel activé (oui/non)

#### **Instance d'Entretien**
- Représente une réalisation concrète d'un entretien
- Attributs :
  - Date de réalisation
  - Coût
  - Prestataire (nom uniquement)
  - Notes/commentaires
  - Statut (réalisé, planifié, en retard)

---

## 3. Fonctionnalités

### 3.1 Fonctionnalités Gratuites

#### Gestion d'une maison
- Créer et gérer 1 maison
- Ajouter jusqu'à **20 appareils** (limitation anti-abus)
- Définir des types d'entretiens personnalisés
- Enregistrer les instances d'entretiens
- Consulter l'historique complet
- Enregistrer les coûts
- Ajouter des prestataires (nom)

#### Rappels et Notifications
- Système de rappels automatiques par email
- Configuration de la périodicité des entretiens
- Alertes pour entretiens à venir

#### Collaboration
- Inviter des collaborateurs sur sa maison
- Permissions de base (lecture ou lecture/écriture)

### 3.2 Fonctionnalités Payantes (Abonnement Mensuel)

#### Gestion Multi-Propriétés
- Accès au niveau "Organisation"
- Création de plusieurs maisons
- Vue d'ensemble de toutes les propriétés
- Gestion centralisée

#### Délégation d'Accès
- Donner accès aux locataires sur des maisons spécifiques
- Gestion fine des permissions par maison

### 3.3 Fonctionnalités Futures (Non incluses dans MVP)
- Stockage de photos et documents (factures, certificats)
- Périodicité légale automatique selon pays/type d'appareil
- Statistiques avancées et budgets
- Informations détaillées sur les prestataires

---

## 4. Gestion des Utilisateurs et Permissions

### 4.1 Rôles

#### **Propriétaire (Owner)**
- Créateur de la maison
- Permissions complètes
- Peut inviter des collaborateurs
- Peut déléguer l'accès aux locataires

#### **Collaborateur**
- Invité par le propriétaire
- Permissions : Lecture seule OU Lecture/Écriture
- Peut consulter et/ou modifier selon permissions

#### **Locataire**
- Accès délégué par le propriétaire
- **Gratuit** (ne paie pas d'abonnement)
- Permissions : Lecture seule OU Lecture/Écriture
- Accès limité à la/les maison(s) spécifique(s)

### 4.2 Matrice de Permissions
| Action | Propriétaire | Collaborateur (R/W) | Collaborateur (R) | Locataire (R/W) | Locataire (R) |
|--------|--------------|---------------------|-------------------|-----------------|---------------|
| Voir maison | ✅ | ✅ | ✅ | ✅ | ✅ |
| Modifier maison | ✅ | ✅ | ❌ | ❌ | ❌ |
| Ajouter appareil | ✅ | ✅ | ❌ | ❌ | ❌ |
| Ajouter type d'entretien | ✅ | ✅ | ❌ | ❌ | ❌ |
| Ajouter instance d'entretien | ✅ | ✅ | ❌ | ✅ | ❌ |
| Modifier instance d'entretien | ✅ | ✅ | ❌ | ✅ | ❌ |
| Supprimer appareil | ✅ | ✅ | ❌ | ❌ | ❌ |
| Inviter collaborateurs | ✅ | ❌ | ❌ | ❌ | ❌ |
| Gérer permissions | ✅ | ❌ | ❌ | ❌ | ❌ |

---

## 5. Modèle de Tarification

### 5.1 Offre Gratuite
- 1 maison
- Appareils et entretiens illimités
- Rappels et notifications
- Collaboration basique

### 5.2 Offre Premium (Abonnement Mensuel)
- **Prix** : À définir
- Multi-propriétés (maisons illimitées)
- Gestion via Organisation
- Délégation aux locataires
- Support prioritaire (optionnel)

### 5.3 Note sur les Locataires
- Les locataires n'ont **jamais** à payer
- Le propriétaire paie l'abonnement pour gérer ses propriétés
- Les locataires obtiennent un accès gratuit délégué

---

## 6. Spécifications Techniques (MVP)

### 6.1 Type d'Application
- **Application web** responsive
- Accessible depuis navigateur desktop et mobile

### 6.2 Fonctionnalités Exclues du MVP
- Stockage de fichiers (photos, documents PDF)
- Application mobile native
- API publique

### 6.3 Système de Rappels
- Notifications par email
- Calcul automatique des prochaines dates selon périodicité
- Rappels configurables (X jours avant échéance)

---

## 7. Architecture Technique Validée

### 7.1 Stack Technique Frontend

**Framework Principal**
- **À choisir (Next.js 14+ ou Blazor)**
- **TypeScript** (si Next.js) ou **C#** (si Blazor)
- **Server-Side Rendering (SSR)** pour les performances

**Design et UI**
- **Tailwind CSS** pour le styling responsive
- **Shadcn/ui** (si React) ou **MudBlazor** (si Blazor)
- **Lucide React** pour les icônes
- **React Hook Form** + **Zod** (si React) pour la validation des formulaires

**State Management**
- **TanStack Query** (si React) ou gestion d'état native Blazor
- **Zustand** ou **Context API** (si React)

### 7.2 Stack Technique Backend

**Framework Principal**
- **ASP.NET Core 8 Web API**
- Langage : **C#**
- Documentation API : **Swagger/OpenAPI** intégrée

**ORM et Base de Données**
- **Entity Framework Core (EF Core)**
- **PostgreSQL** comme base de données relationnelle
- Migrations gérées par EF Core
- Fortement typé pour minimiser les erreurs d'exécution

**Hébergement & Orchestration**
- **.NET Aspire** pour l'orchestration locale et cloud.
- **Docker** pour la conteneurisation et la portabilité totale.
- **Azure Container Apps** pour l'hébergement cloud scale-to-zero.

**Sécurité Backend**
- JWT Bearer Authentication
- ASP.NET Core Identity pour la gestion des utilisateurs
- Policies-based Authorization pour la matrice de permissions

### 7.3 Authentification et Sécurité

**Système d'Authentification**
- **ASP.NET Core Identity**
- Email/Password (credentials provider)
- OAuth optionnel (Google, GitHub)
- Sessions JWT

**Sécurité**
- HTTPS obligatoire
- Rate limiting on endpoints sensibles
- Validation des inputs
- Protection SQL Injection (EF Core)
- XSS Protection
- CSRF Protection
- Passwords hashés cryptographiquement

**Conformité RGPD**
- Consentement explicite pour emails marketing
- Droit à l'oubli (suppression des données)
- Export de données utilisateur
- Politique de confidentialité
- Banner de consentement cookies

### 7.4 Système de Paiement

**Solution**
- **Stripe** pour les abonnements récurrents
- **Stripe.NET** pour l'intégration backend
- Stripe Checkout pour le paiement
- Stripe Customer Portal pour la gestion de l'abonnement
- Webhooks pour synchronisation automatique

### 7.5 Système de Notifications Email

**Service Email**
- **Resend** (recommandé) ou **SendGrid**
- 3,000 emails/mois gratuits (Resend)

**Types d'Emails**
- Emails d'authentification (vérification, reset password)
- Emails d'invitation (collaborateurs, locataires)
- Rappels d'entretien automatiques

**Gestion du Multilingue (Communications)**
- Support **Français et Anglais** pour le MVP.
- Langue préférée stockée dans le profil `User`.
- Localisation côté backend pour les communications (Emails, Notifications) via fichiers de ressources `.resx` ou templates localisés.
- Architecture extensible pour ajout facile de nouvelles langues.

**Système de Rappels**
- Job planifié (Background Service ou Hangfire)
- Vérification des entretiens à venir
- Envoi automatique selon configuration utilisateur

### 7.6 Infrastructure et Déploiement

**Orchestration & Infrastructure**
- **.NET Aspire** : Orchestration des services et ressources
- **Docker** : Conteneurisation de tous les services
- **Azure Container Apps** : Déploiement cloud simplifié (via Azure Developer CLI - `azd`)

**Hébergement Frontend**
- **Azure Container Apps** (via Docker)

**Environnements & CI/CD**
- **Azure Developer CLI (`azd`)** pour le provisionnement et le déploiement.
- **GitHub Actions** pour le CI/CD automatisé.
- **Environnements** : Production (branche `main`), Staging (branche `develop`), Preview (chaque Pull Request).

### 7.7 Schéma de Base de Données

**Entités Principales**
```
User (utilisateurs)
  ↓
Organization (niveau payant, multi-propriétés)
  ↓
House (maisons)
  ↓
HouseAccess (permissions par maison)
  ↓
Device (appareils)
  ↓
MaintenanceType (types d'entretien)
  ↓
MaintenanceInstance (réalisations)
```

**Gestion des Permissions**
- Table `HouseAccess` avec rôles (OWNER, COLLABORATOR, TENANT)
- Permissions (READ, READ_WRITE)
- Vérification au niveau API (Authorize attribute / Policy)

### 7.8 Estimation des Coûts (Infrastructure)

**Phase MVP (Tier Gratuits / Crédits Azure)**
- Azure Container Apps (Scale to zero)
- Azure Database for PostgreSQL (ou Postgres conteneurisé sur ACA)
- Resend (Free)
- Stripe (Commission uniquement)

**Total estimé MVP**: 0€/mois

---

## 8. Parcours Utilisateurs Types

### 7.1 Propriétaire Individuel (Gratuit)
1. S'inscrit sur la plateforme
2. Crée sa première maison
3. Ajoute ses appareils (chaudière, poêle, alarmes)
4. Configure les types d'entretiens avec périodicité
5. Reçoit des rappels automatiques
6. Enregistre les entretiens réalisés avec coûts

### 7.2 Propriétaire Multi-Propriétés (Payant)
1. S'inscrit en offre gratuite
2. Décide de gérer plusieurs propriétés
3. Souscrit à l'abonnement mensuel
4. Accède au niveau "Organisation"
5. Crée plusieurs maisons
6. Délègue l'accès à des locataires avec permissions spécifiques

### 7.3 Locataire (Gratuit)
1. Reçoit une invitation du propriétaire
2. Création de compte (gratuit)
3. Accède à la maison déléguée
4. Consulte ou ajoute des entretiens selon permissions

---

## 9. Questions en Suspens / À Définir

1. **Nom de l'application** : À définir
2. **Prix de l'abonnement mensuel** : À définir
3. **Design et charte graphique** : À définir
4. **Langues** : Français et Anglais gérés (MVP).
5. **Frontend** : Blazor ou Next.js ?

---

## 10. Prochaines Étapes

### Phase 1 : Validation et Préparation
1. ✅ Cahier des charges fonctionnel validé
2. ✅ Architecture technique validée (.NET Backend)
3. ⏳ Choix final du frontend (Blazor vs React)
4. ⏳ Définition du design (wireframes, mockups UI/UX)

### Phase 2 : Setup Technique
1. Création de la solution .NET 8
2. Configuration Entity Framework Core + PostgreSQL
3. Setup ASP.NET Core Identity
4. Initialisation du frontend choisi
5. Mise en place CI/CD avec **GitHub Actions** et **azd**

### Phase 3 : Développement MVP
1. **API Authentification**
2. **API Gestion des Maisons**
3. **API Appareils et Entretiens**
4. **Gestion des Permissions**
5. **Intégration Stripe.NET**
6. **Service de Notifications Email**

---

**Version** : 2.2 - Architecture .NET Aspire & Azure Container Apps village
**Date** : 2025-12-23  
**Statut** : Validé - Backend .NET Aspire & Azure confirmé
**Architecte** : Antigravity AI
