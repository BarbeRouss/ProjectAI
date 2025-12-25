@echo off
REM Script d'installation HouseFlow Frontend
REM Execute ce script pour installer toutes les dependances

echo ========================================
echo  HouseFlow Frontend - Installation
echo ========================================
echo.

REM Verification Node.js
echo [1/4] Verification Node.js...
node --version >nul 2>&1
if errorlevel 1 (
    echo [ERREUR] Node.js n'est pas installe!
    echo.
    echo Telechargez Node.js 20+ depuis: https://nodejs.org/
    echo.
    pause
    exit /b 1
)

node --version
npm --version
echo [OK] Node.js est installe
echo.

REM Installation des dependances
echo [2/4] Installation des dependances npm...
echo Cette etape peut prendre 2-3 minutes...
echo.
npm install
if errorlevel 1 (
    echo [ERREUR] Echec de l'installation npm
    pause
    exit /b 1
)
echo [OK] Dependances installees
echo.

REM Installation Playwright browsers
echo [3/4] Installation des navigateurs Playwright...
echo Cette etape peut prendre 3-5 minutes...
echo.
npx playwright install
if errorlevel 1 (
    echo [AVERTISSEMENT] Echec de l'installation Playwright
    echo Les tests E2E ne pourront pas s'executer
) else (
    echo [OK] Navigateurs Playwright installes
)
echo.

REM Verification
echo [4/4] Verification de l'installation...
if exist "node_modules" (
    echo [OK] node_modules/ cree
) else (
    echo [ERREUR] node_modules/ introuvable
)

if exist "node_modules\.bin\next" (
    echo [OK] Next.js installe
) else (
    echo [ERREUR] Next.js introuvable
)

if exist "node_modules\.bin\playwright" (
    echo [OK] Playwright installe
) else (
    echo [AVERTISSEMENT] Playwright introuvable
)
echo.

echo ========================================
echo  Installation Terminee!
echo ========================================
echo.
echo Prochaines etapes:
echo.
echo 1. Demarrer Aspire:
echo    dotnet run --project ..\HouseFlow.AppHost
echo.
echo 2. OU demarrer frontend seul:
echo    npm run dev
echo.
echo 3. Lancer les tests Playwright:
echo    npm test
echo.
echo Frontend accessible a: http://localhost:3000
echo.
pause
