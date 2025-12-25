# HouseFlow Frontend

Next.js 15 frontend for HouseFlow, integrated with .NET Aspire orchestration.

## Stack

- **Framework:** Next.js 15 (App Router)
- **Language:** TypeScript 5.7
- **Styling:** Tailwind CSS 4 + Shadcn/ui
- **State Management:** TanStack Query v5 + Zustand
- **i18n:** next-intl (FR/EN)
- **Theme:** next-themes (Dark mode)
- **Testing:** Playwright E2E
- **API Client:** Auto-generated from OpenAPI

## Prerequisites

- Node.js 20+ and npm
- .NET 10 SDK
- Running HouseFlow.API (via Aspire)

## Getting Started

### 1. Install Dependencies

```bash
npm install
```

### 2. Generate API Client

Make sure the API is running first (via Aspire), then:

```bash
npm run generate-client
```

This will generate the TypeScript API client from the OpenAPI specification at `http://localhost:5203/swagger/v1/swagger.json`.

### 3. Development Server

**Option A: Standalone (requires API running separately)**
```bash
npm run dev
```

**Option B: Via Aspire (recommended)**
```bash
# From repository root
dotnet run --project src/HouseFlow.AppHost
```

The frontend will be available at `http://localhost:3000`.

## Project Structure

```
src/
├── app/[locale]/          # Next.js App Router with i18n
│   ├── (auth)/           # Auth pages (login, register)
│   ├── (dashboard)/      # Protected dashboard pages
│   └── layout.tsx        # Root layout with providers
├── components/
│   ├── ui/               # Shadcn/ui components
│   ├── providers/        # React context providers
│   └── ...               # Feature components
├── lib/
│   ├── api/              # Generated OpenAPI client + hooks
│   ├── auth/             # Auth context
│   ├── i18n/             # Internationalization config
│   ├── utils/            # Utility functions
│   └── validations/      # Zod schemas
└── messages/             # i18n translations (en.json, fr.json)

e2e/
├── fixtures/             # Playwright fixtures (auth, db)
├── pages/                # Page Object Models
└── tests/                # E2E test suites
```

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build production bundle
- `npm run start` - Start production server
- `npm run lint` - Run ESLint
- `npm run generate-client` - Generate API client from OpenAPI spec
- `npm test` - Run Playwright tests
- `npm run test:ui` - Run Playwright tests in UI mode
- `npm run test:debug` - Debug Playwright tests

## Environment Variables

The following environment variables are automatically injected by Aspire:

- `services__api__https__0` - HTTPS API endpoint URL
- `services__api__http__0` - HTTP API endpoint URL (fallback)

These are configured in `next.config.ts` and exposed as `NEXT_PUBLIC_API_URL`.

## Aspire Integration

This project is designed to run as part of the HouseFlow Aspire orchestration. The `.csproj` file enables Aspire to:

1. Manage the Node.js process lifecycle
2. Inject service discovery environment variables
3. Coordinate with the API and database services
4. Provide unified logging and monitoring

See `HouseFlow.AppHost/Program.cs` for the Aspire configuration.

## Testing

### E2E Tests with Playwright

```bash
# Run all tests
npm test

# Run with UI
npm run test:ui

# Debug mode
npm run test:debug

# Run specific test file
npx playwright test e2e/tests/onboarding.spec.ts
```

Tests cover:
- ✅ User flows (Onboarding, Devices, Maintenance, Collaboration)
- ✅ Error scenarios (validation, auth, API errors)
- ✅ Cross-browser (Chromium, Firefox, WebKit, Mobile)

## Internationalization

The app supports French (default) and English. To add translations:

1. Edit `src/messages/fr.json` and `src/messages/en.json`
2. Use `useTranslations()` hook in components:

```tsx
import { useTranslations } from 'next-intl';

export function MyComponent() {
  const t = useTranslations('dashboard');
  return <h1>{t('title')}</h1>;
}
```

## Dark Mode

Dark mode is enabled by default using `next-themes`. Users can toggle between light, dark, and system themes.

## License

Proprietary - HouseFlow Project
