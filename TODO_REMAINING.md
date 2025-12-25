# TODO - Fonctionnalités Restantes

## ⚠️ Actions Requises

### 1. Installation Frontend
```bash
cd src/HouseFlow.Frontend
npm install
npx playwright install
```

## 🎨 UI à Compléter

### Composants Shadcn/ui Manquants
```bash
npx shadcn-ui@latest add dialog
npx shadcn-ui@latest add select
npx shadcn-ui@latest add input
npx shadcn-ui@latest add label
npx shadcn-ui@latest add toast
```

### Features UI

**1. Navigation Header** (45 min)
- Créer `src/components/layout/header.tsx`
- Logo, navigation, user menu
- Theme toggle, locale switcher, logout

**2. Invite Member Dialog** (30 min)
- Créer `src/components/houses/invite-member-dialog.tsx`
- Intégrer dans `houses/[id]/page.tsx`
- Utiliser `useInviteMember` hook (déjà créé)

**3. Add Maintenance Type Dialog** (30 min)
- Créer `src/components/maintenance/add-maintenance-type-dialog.tsx`
- Utiliser `useCreateMaintenanceType` hook

**4. Theme Toggle Button** (15 min)
```tsx
// src/components/shared/theme-toggle.tsx
import { useTheme } from 'next-themes';
export function ThemeToggle() {
  const { theme, setTheme } = useTheme();
  return <button onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}>
    {theme === 'dark' ? '☀️' : '🌙'}
  </button>;
}
```

**5. Locale Switcher** (15 min)
```tsx
// src/components/shared/locale-switcher.tsx
import { useRouter } from 'next/navigation';
export function LocaleSwitcher() {
  const router = useRouter();
  return <div>
    <button onClick={() => router.push('/fr/...')}>FR</button>
    <button onClick={() => router.push('/en/...')}>EN</button>
  </div>;
}
```

## 🧪 Tests à Ajouter

**E2E Tests manquants:**
- Logout flow
- Edit house
- Delete device
- Form validation errors
- Network error handling

**Frontend Unit Tests:**
- Components tests (Jest/Vitest)
- Hooks tests
- Utils tests

## 📈 Améliorations Optionnelles

**UX:**
- Loading skeletons
- Error boundaries
- Empty states avec illustrations
- Animations (Framer Motion)

**Features:**
- Upload photos/documents
- Dashboard statistics/graphs
- Search & filters
- Notifications/reminders

**Performance:**
- Image optimization
- Code splitting
- Bundle size analysis

## 🔧 Configuration

**Environnement variables (.env.local):**
```bash
NEXT_PUBLIC_API_URL=http://localhost:5203
```

**Aspire AppHost:**
- Frontend déjà configuré
- Variables auto-injectées

## 📝 Notes

- ✅ Backend: 100% fonctionnel
- ✅ Frontend: 90% fonctionnel
- ⏳ UI polish: 70%
- ⏳ Tests E2E: Prêts mais non exécutés (npm install requis)
