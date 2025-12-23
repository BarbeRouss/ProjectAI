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
- **Next.js 14+** (React avec App Router)
- **TypeScript** pour la robustesse du code
- **Server-Side Rendering (SSR)** pour les performances

**Design et UI**
- **Tailwind CSS** pour le styling responsive
- **Shadcn/ui** pour les composants réutilisables et accessibles
- **Lucide React** pour les icônes
- **React Hook Form** + **Zod** pour la validation des formulaires

**State Management**
- **TanStack Query (React Query)** pour la gestion du cache et des requêtes API
- **Zustand** ou **Context API** pour l'état global

### 7.2 Stack Technique Backend

**API et Serveur**
- **Next.js API Routes** (full-stack dans le même projet)
- Architecture RESTful
- Type safety de bout en bout avec TypeScript

**ORM et Base de Données**
- **Prisma ORM** pour l'accès à la base de données
- **PostgreSQL** comme base de données relationnelle
- Migrations automatiques et type-safety

**Hébergement Base de Données**
- **Neon** (PostgreSQL serverless) ou
- **Supabase** (PostgreSQL + fonctionnalités avancées) ou
- **Vercel Postgres** (intégration native)

### 7.3 Authentification et Sécurité

**Système d'Authentification**
- **NextAuth.js v5 (Auth.js)**
- Email/Password (credentials provider)
- OAuth optionnel (Google, GitHub)
- Sessions JWT avec stockage en base de données

**Sécurité**
- HTTPS obligatoire (Vercel)
- Rate limiting on endpoints sensibles
- Validation des inputs avec Zod
- Protection SQL Injection (Prisma ORM)
- Protection XSS (React escape automatique)
- CSRF tokens (NextAuth)
- Passwords hashés avec bcrypt

**Conformité RGPD**
- Consentement explicite pour emails marketing
- Droit à l'oubli (suppression des données)
- Export de données utilisateur
- Politique de confidentialité
- Banner de consentement cookies

### 7.4 Système de Paiement

**Solution**
- **Stripe** pour les abonnements récurrents
- Stripe Checkout pour le paiement
- Stripe Customer Portal pour la gestion de l'abonnement
- Webhooks pour synchronisation automatique

**Flux de Paiement**
1. Utilisateur clique "Upgrade to Premium"
2. Création de Stripe Checkout Session
3. Paiement sur page Stripe hébergée
4. Webhook confirme le paiement
5. Activation de l'Organisation en base de données

**Webhooks Stripe**
- `checkout.session.completed` → Création Organisation/Subscription
- `invoice.paid` → Renouvellement abonnement
- `invoice.payment_failed` → Suspension compte
- `customer.subscription.deleted` → Annulation abonnement

### 7.5 Système de Notifications Email

**Service Email**
- **Resend** (recommandé) ou **SendGrid**
- API moderne avec templates React/JSX
- 3,000 emails/mois gratuits (Resend)

**Types d'Emails**
- Emails d'authentification (vérification, reset password)
- Emails d'invitation (collaborateurs, locataires)
- Rappels d'entretien automatiques

**Système de Rappels**
- **Vercel Cron** pour job quotidien automatique
- Vérification des entretiens à venir
- Envoi automatique selon configuration utilisateur
- Tracking pour éviter les doublons

### 7.6 Infrastructure et Déploiement

**Hébergement**
- **Vercel** pour l'application Next.js
- Déploiement automatique depuis GitHub
- CDN global
- SSL automatique
- Preview deployments pour chaque PR

**CI/CD**
- **GitHub Actions** pour linting et tests
- Déploiement automatique sur merge

**Environnements**
- Production (branche `main`)
- Staging (branche `develop`)
- Preview (chaque Pull Request)

**Monitoring**
- **Vercel Analytics** (performances)
- **Sentry** (tracking des erreurs)
- Monitoring natif de la base de données

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
- Vérification à chaque requête API

**Subscription**
- Lien avec Stripe Customer ID
- Status de l'abonnement
- Date de fin de période

### 7.8 Estimation des Coûts (Infrastructure)

**Phase MVP (Tier Gratuits)**
- Vercel (Hobby): 0€/mois
- Neon/Supabase (Free): 0€/mois
- Resend (3k emails/mois): 0€/mois
- Stripe: 1,5% + 0,25€ par transaction
- Sentry (limité): 0€/mois

**Total estimé MVP**: 0€/mois

**Phase Croissance (1000+ utilisateurs actifs)**
- Vercel (Pro): ~20€/mois
- Base de données: ~25€/mois
- Resend: ~20€/mois
- Stripe: selon volume
- Sentry: ~26€/mois

**Total estimé croissance**: 90-120€/mois (hors Stripe)

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
4. **Langues** : Français uniquement ou multilingue ?
5. **Fréquence des rappels** : Paramétrable par utilisateur ?
6. **Limite du nombre de collaborateurs** : Par maison ?
7. **Processus d'invitation** : Email avec lien d'invitation ?

---

## 10. Prochaines Étapes

### Phase 1 : Validation et Préparation
1. ✅ Cahier des charges fonctionnel validé
2. ✅ Architecture technique validée
3. ⏳ Validation du budget et coûts d'infrastructure
4. ⏳ Définition du design (wireframes, mockups UI/UX)

### Phase 2 : Setup Technique
1. Initialisation du projet Next.js + TypeScript
2. Configuration Prisma + PostgreSQL (Neon/Supabase)
3. Setup NextAuth.js pour l'authentification
4. Configuration Tailwind CSS + Shadcn/ui
5. Mise en place CI/CD (GitHub Actions + Vercel)

### Phase 3 : Développement MVP
1. **Module Authentification**
   - Inscription/Connexion
   - Gestion de profil
   - Reset password

2. **Module Maisons**
   - CRUD Maisons
   - Gestion des collaborateurs
   - Système d'invitation

3. **Module Appareils et Entretiens**
   - CRUD Appareils
   - Types d'entretien avec périodicité
   - Instances d'entretien (historique)
   - Calcul des prochaines dates

4. **Module Permissions**
   - Matrice de permissions
   - Vérification des accès
   - Gestion des rôles (Owner, Collaborator, Tenant)

5. **Module Abonnements (Stripe)**
   - Intégration Stripe Checkout
   - Gestion Organisation (niveau payant)
   - Webhooks Stripe
   - Limitation 1 maison (gratuit) vs illimitée (payant)
   - Customer Portal

6. **Module Notifications**
   - Intégration Resend/SendGrid
   - Templates d'emails
   - Système de rappels automatiques (Cron)
   - Configuration des notifications par utilisateur

### Phase 4 : Tests et Qualité
1. Tests unitaires (composants React)
2. Tests d'intégration (API)
3. Tests de sécurité et permissions
4. Validation RGPD
5. Tests utilisateurs (beta testers)

### Phase 5 : Déploiement et Lancement
1. Déploiement en staging
2. Tests finaux
3. Configuration DNS et domaine
4. Déploiement en production
5. Monitoring et analytics
6. Lancement public

---

**Version** : 2.0 - Architecture Technique Intégrée  
**Date** : 2025-12-23  
**Statut** : Validé - Prêt pour développement  
**Architecte** : Antigravity AI
