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

## 8. Questions en Suspens / À Définir

1. **Nom de l'application** : À définir
2. **Prix de l'abonnement mensuel** : À définir
3. **Design et charte graphique** : À définir
4. **Langues** : Français uniquement ou multilingue ?
5. **Fréquence des rappels** : Paramétrable par utilisateur ?
6. **Limite du nombre de collaborateurs** : Par maison ?
7. **Processus d'invitation** : Email avec lien d'invitation ?

---

## 9. Prochaines Étapes

1. Validation du cahier des charges
2. Définition du design (wireframes, mockups)
3. Choix de la stack technique
4. Développement du MVP
5. Tests utilisateurs
6. Mise en production

---

**Version** : 1.0  
**Date** : 2025-12-23  
**Statut** : Draft - En cours de validation
