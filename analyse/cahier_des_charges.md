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

## 6. Spécifications Générales (MVP)

### 6.1 Informations Générales
- **Nom de l'application** : À définir (en collaboration avec le PM).
- **Prix de l'abonnement** : À définir (en collaboration avec le PM).

### 6.2 Interface et Expérience Utilisateur
- **Type** : Application web responsive (Desktop & Mobile).
- **Frontend** : Next.js 14+.
- **Design Style** : "Consumer Moderne" (Premium, vibrant, micro-animations).
- **Langues** : Support du **français** et de l'**anglais** dès le lancement.

### 6.3 Système de Rappels
- Notifications par email.
- Rappels configurables (X jours avant échéance).

---

## 7. Parcours Utilisateurs Types

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

## 8. Choix de Design et Identité

1. **Nom de l'application** : À définir (en collaboration avec le PM).
2. **Style visuel** : **Consumer Moderne**. Focus sur la simplicité d'utilisation alliée à une esthétique haut de gamme (vibrant, animations, premium).
3. **Paiement** : Intégration **Stripe** pour les abonnements Premium.

---

## 9. Prochaines Étapes

1. ✅ Cahier des charges fonctionnel validé.
2. ✅ Architecture technique validée.
3. ⏳ Initialisation de l'environnement de développement (.NET Aspire).
4. ⏳ Développement du MVP.

---

**Version** : 1.1 - Validé
**Date** : 2025-12-23
**Statut** : Validé
**Auteur** : Antigravity AI
