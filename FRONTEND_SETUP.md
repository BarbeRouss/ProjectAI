# Frontend Setup

Frontend Next.js 15 avec intégration Aspire.

## Stack

- Next.js 15 (App Router)
- TypeScript 5.7
- React 19
- Tailwind CSS 4
- TanStack Query v5
- Playwright (tests E2E)

## Installation

```bash
cd src/HouseFlow.Frontend
npm install
npx playwright install
```

## Démarrage

**Via Aspire (auto-start):**
```bash
dotnet run --project src/HouseFlow.AppHost
```

**Standalone:**
```bash
npm run dev
```

## Configuration

### Variables d'environnement

Injectées automatiquement par Aspire dans `next.config.ts`:
- `services__api__https__0` - API URL (HTTPS)
- `services__api__http__0` - API URL (HTTP)

### API Client

**Générer le client TypeScript:**
```bash
npm run generate-client
```

Note: Les hooks React Query sont déjà implémentés manuellement dans `src/lib/api/hooks/`

## Structure

```
src/
├── app/[locale]/          # Pages avec i18n
│   ├── (auth)/           # Login, Register
│   └── (dashboard)/      # Dashboard, Houses, Devices
├── components/
│   ├── ui/               # Button, Card, etc.
│   ├── auth/             # Formulaires auth
│   └── maintenance/      # Composants maintenance
├── lib/
│   ├── api/
│   │   ├── client.ts     # Axios + interceptors
│   │   └── hooks/        # React Query hooks
│   ├── auth/             # Auth context
│   └── i18n/             # i18n config
└── messages/             # Traductions FR/EN
```

## Tests E2E

```bash
npm test              # Tous les tests
npm run test:ui       # Mode interactif
npm run test:debug    # Debug
```

11 tests Playwright couvrant 4 flux utilisateurs.

## Commandes

```bash
npm run dev           # Dev server
npm run build         # Build production
npm run start         # Start production
npm run lint          # ESLint
npm test              # Tests Playwright
npm run generate-client  # Générer client API
```

## Dark Mode

```tsx
import { useTheme } from 'next-themes';

const { theme, setTheme } = useTheme();
setTheme('dark'); // 'light' | 'dark' | 'system'
```

## i18n

Changer locale:
```
/fr/dashboard → Français
/en/dashboard → English
```

Utiliser dans composants:
```tsx
import { useTranslations } from 'next-intl';

const t = useTranslations('dashboard');
<h1>{t('welcome')}</h1>
```

## Dépannage

**Port 3000 occupé:**
```bash
lsof -ti:3000 | xargs kill -9
```

**Build errors:**
```bash
rm -rf node_modules .next
npm install
```
