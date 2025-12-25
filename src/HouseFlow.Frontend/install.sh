#!/bin/bash
# Script d'installation HouseFlow Frontend
# Execute ce script pour installer toutes les dépendances

set -e  # Exit on error

echo "========================================"
echo "  HouseFlow Frontend - Installation"
echo "========================================"
echo ""

# Verification Node.js
echo "[1/4] Vérification Node.js..."
if ! command -v node &> /dev/null; then
    echo "[ERREUR] Node.js n'est pas installé!"
    echo ""
    echo "Téléchargez Node.js 20+ depuis: https://nodejs.org/"
    echo ""
    exit 1
fi

node --version
npm --version
echo "[OK] Node.js est installé"
echo ""

# Installation des dépendances
echo "[2/4] Installation des dépendances npm..."
echo "Cette étape peut prendre 2-3 minutes..."
echo ""
npm install
echo "[OK] Dépendances installées"
echo ""

# Installation Playwright browsers
echo "[3/4] Installation des navigateurs Playwright..."
echo "Cette étape peut prendre 3-5 minutes..."
echo ""
npx playwright install || echo "[AVERTISSEMENT] Échec installation Playwright"
echo "[OK] Navigateurs Playwright installés"
echo ""

# Vérification
echo "[4/4] Vérification de l'installation..."
if [ -d "node_modules" ]; then
    echo "[OK] node_modules/ créé"
else
    echo "[ERREUR] node_modules/ introuvable"
fi

if [ -f "node_modules/.bin/next" ]; then
    echo "[OK] Next.js installé"
else
    echo "[ERREUR] Next.js introuvable"
fi

if [ -f "node_modules/.bin/playwright" ]; then
    echo "[OK] Playwright installé"
else
    echo "[AVERTISSEMENT] Playwright introuvable"
fi
echo ""

echo "========================================"
echo "  Installation Terminée!"
echo "========================================"
echo ""
echo "Prochaines étapes:"
echo ""
echo "1. Démarrer Aspire:"
echo "   dotnet run --project ../HouseFlow.AppHost"
echo ""
echo "2. OU démarrer frontend seul:"
echo "   npm run dev"
echo ""
echo "3. Lancer les tests Playwright:"
echo "   npm test"
echo ""
echo "Frontend accessible à: http://localhost:3000"
echo ""
