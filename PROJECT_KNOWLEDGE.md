# HouseFlow - Project Knowledge Base

**Last Updated**: 2025-12-25

## Project Overview

**HouseFlow** is a home maintenance tracking application that helps users manage multiple properties, devices, and maintenance schedules. Built with .NET 10 backend and Next.js 15 frontend.

## Technology Stack

### Backend
- **.NET 10** with C# 13
- **ASP.NET Core Web API**
- **Entity Framework Core 10** with PostgreSQL
- **Aspire 13.1.0** for orchestration and observability
- **NSwag** for OpenAPI/Swagger documentation
- **JWT** for authentication
- **BCrypt.Net** for password hashing
- **Onion Architecture** (Clean Architecture)

### Frontend
- **Next.js 15.5.9** with App Router
- **React 19**
- **TypeScript** (strict mode)
- **Tailwind CSS v3.4.19**
- **Shadcn/ui** components
- **TanStack Query** for data fetching
- **Zustand** for state management
- **next-intl** for internationalization (French/English)
- **Playwright 1.57.0** for E2E testing (70 tests)

### Infrastructure
- **PostgreSQL 16** for database
- **Docker** for containerization
- **Azure Container Apps** (deployment target)
- **Azure Database for PostgreSQL**

## Architecture

### Onion Architecture Layers

```
src/
├── HouseFlow.Core/              # Domain entities and interfaces
│   ├── Entities/                # House, Device, User, etc.
│   └── Enums/                   # HouseRole, InvitationStatus
├── HouseFlow.Application/       # Business logic and DTOs
│   ├── DTOs/                    # Data Transfer Objects
│   └── Services/                # Business services
├── HouseFlow.Infrastructure/    # Data access and external services
│   ├── Persistence/             # EF Core DbContext
│   ├── Repositories/            # Repository implementations
│   └── Services/                # AuthService, HouseService, etc.
├── HouseFlow.API/              # REST API controllers
│   └── Controllers/             # API endpoints
├── HouseFlow.AppHost/          # Aspire orchestration
└── HouseFlow.Frontend/         # Next.js application
```

### API-First Development Workflow

**CRITICAL**: This project follows an **API-First (Contract-First)** approach:

1. **Update OpenAPI Spec** (`analyse_technique/openapi.yaml`)
2. **Regenerate Frontend Client**:
   ```bash
   cd src/HouseFlow.Frontend
   npm run generate-client
   ```
3. **Update Backend Code** manually to match spec:
   - DTOs in `Application/DTOs/`
   - Entities in `Core/Entities/`
   - Services in `Infrastructure/Services/`
4. **Run Tests** to verify everything works

**Source of Truth**: `analyse_technique/openapi.yaml`

## Key Features Implemented

### Authentication & Onboarding
- User registration with email/password
- JWT-based authentication
- Auto-creates first house named "Ma Maison" on registration
- Redirects to device creation page after registration
- Single house auto-redirect: users with only 1 house are automatically redirected to it

### House Management
- Create, view, and manage houses
- Optional address fields (address, zipCode, city)
- Invite members (Owner, Collaborator, Tenant roles)
- View house details with member list

### Device Management
- Add devices to houses
- Device types: Chaudière Gaz, Pompe à Chaleur, etc.
- Optional install date
- View device details

### Maintenance Tracking
- Define maintenance types (Annual, Semestrial, etc.)
- Log maintenance instances
- View maintenance history

## Database Schema

### Key Entities

**User**
- Id (Guid)
- Email (unique)
- Name
- PasswordHash

**Organization**
- Id (Guid)
- Name
- OwnerId → User

**House**
- Id (Guid)
- Name (required)
- Address (optional)
- ZipCode (optional)
- City (optional)
- OrganizationId → Organization

**HouseMember**
- Id (Guid)
- HouseId → House
- UserId → User
- Role (Owner, Collaborator, Tenant)
- Status (Pending, Accepted)

**Device**
- Id (Guid)
- Name
- Type
- InstallDate (optional)
- HouseId → House

**MaintenanceType**
- Id (Guid)
- Name
- Periodicity (Annual, Semestrial, etc.)
- DeviceId → Device

**MaintenanceInstance**
- Id (Guid)
- Date
- Cost
- Provider
- Notes
- Status
- MaintenanceTypeId → MaintenanceType

## Design System

### Color Palette (from wireframes)

**Light Mode**:
- Primary: `hsl(239, 84%, 67%)` - Indigo/blue
- Background: `hsl(0, 0%, 98%)` - Off-white
- Card: `hsl(0, 0%, 100%)` - White
- Text: `hsl(222.2, 84%, 4.9%)` - Dark navy
- Muted: `hsl(220, 9%, 46%)` - Gray

**Dark Mode**:
- Primary: `hsl(239, 84%, 67%)` - Same indigo
- Background: `hsl(224, 71%, 4%)` - Dark navy
- Card: `hsl(224, 71%, 4%)` - Dark navy
- Text: `hsl(213, 31%, 91%)` - Light gray

### Design Specs
- **Border Radius**: `0.75rem` (12px) for rounded corners
- **Typography**: Clean, modern sans-serif
- **Shadows**: Subtle drop shadows on cards
- **Spacing**: Generous whitespace
- **Icons**: Simple line icons with circular backgrounds

## Internationalization (i18n)

**Languages**: French (fr) and English (en)

**Translation Files**:
- `src/HouseFlow.Frontend/src/messages/fr.json`
- `src/HouseFlow.Frontend/src/messages/en.json`

**Usage**:
```tsx
const t = useTranslations('namespace');
const tCommon = useTranslations('common');
```

**Namespaces**:
- `common`: loading, error, save, cancel, viewDetails, optional, etc.
- `auth`: login, register, email, password, etc.
- `dashboard`: welcome, myHouses, noHousesYet, etc.
- `houses`: title, addHouse, members, notFound, etc.
- `devices`: title, addDevice, noDevicesYet, createError, etc.
- `maintenance`: title, logMaintenance, history, etc.

## Running the Application

### Prerequisites
- .NET 10 SDK
- Node.js 20+
- PostgreSQL 16 (optional, Aspire can start it)

### Development Mode

**Via Aspire (Recommended)**:
```bash
dotnet run --project src/HouseFlow.AppHost
```
This starts:
- PostgreSQL (port 5432)
- HouseFlow.API (port 5203)
- HouseFlow.Frontend (port 3000)
- Aspire Dashboard (port 15000)

**Manual Mode**:
```bash
# Terminal 1: Backend
cd src/HouseFlow.API
dotnet run

# Terminal 2: Frontend
cd src/HouseFlow.Frontend
npm run dev
```

### Testing

**Backend Tests**:
```bash
dotnet test
```

**Frontend E2E Tests**:
```bash
cd src/HouseFlow.Frontend
npm test              # All tests
npm run test:ui       # Interactive mode
npm run test:debug    # Debug mode
```

**Current Test Status**: 23/70 tests passing (need to fix 47 failing tests)

## Recent Changes (2025-12-25)

### API-First Workflow Implementation
1. Updated `openapi.yaml`:
   - Made house address fields optional (address, zipCode, city)
   - Added `firstHouseId` to `AuthResponse`
2. Regenerated frontend TypeScript client from OpenAPI
3. Updated backend DTOs and entities to match spec
4. Documented API-First workflow in README.md

### UI/UX Improvements
1. Analyzed wireframes for design specifications
2. Updated color palette to indigo-based theme
3. Increased border radius from 0.5rem to 0.75rem
4. Updated both light and dark mode color schemes

### Internationalization Fixes
1. Added missing translation keys to en.json and fr.json
2. Updated components to use translation keys:
   - `houses/[id]/page.tsx`: Fixed all hardcoded English
   - `houses/[id]/devices/new/page.tsx`: Fixed error messages and select options
   - `dashboard/page.tsx`: Fixed all UI text
3. Eliminated English/French mixing throughout the application

### Feature Implementations
1. **Auto-Create First House**: On registration, creates "Ma Maison"
2. **Single House Auto-Redirect**: Users with 1 house are redirected to house details
3. **Optional Address Fields**: Address, zipCode, city no longer required
4. **Device Creation Flow**: Redirects to device creation after registration

## Known Issues

### Test Failures (47/70 failing)
1. **Onboarding Tests**: Expect old flow (manual house creation)
2. **Device Management Tests**: Locale prefix issues causing timeouts
3. **Maintenance Tests**: Locale prefix issues causing timeouts

**Root Cause**: Tests were written for old flow before:
- Auto-creation of first house
- Redirect to device creation
- Optional address fields

**Fix Required**: Update all E2E tests to match new user flow

## File Locations

### Configuration
- OpenAPI Spec: `analyse_technique/openapi.yaml`
- Frontend Config: `src/HouseFlow.Frontend/openapi-ts.config.ts`
- Tailwind Config: `src/HouseFlow.Frontend/tailwind.config.ts`
- i18n Messages: `src/HouseFlow.Frontend/src/messages/{fr,en}.json`

### Key Backend Files
- Auth Service: `src/HouseFlow.Infrastructure/Services/AuthService.cs`
- House Service: `src/HouseFlow.Infrastructure/Services/HouseService.cs`
- DTOs: `src/HouseFlow.Application/DTOs/`
- Entities: `src/HouseFlow.Core/Entities/`

### Key Frontend Files
- API Client: `src/HouseFlow.Frontend/src/lib/api/generated/`
- Hooks: `src/HouseFlow.Frontend/src/lib/api/hooks/`
- Pages: `src/HouseFlow.Frontend/src/app/[locale]/(dashboard)/`
- Styles: `src/HouseFlow.Frontend/src/app/globals.css`

## Environment Variables

```env
# Backend (appsettings.Development.json)
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=houseflow;Username=postgres;Password=yourpassword
Jwt__Key=YourSuperSecretKeyForJWTTokenGeneration123456
Jwt__Issuer=HouseFlowAPI
Jwt__Audience=HouseFlowClient

# Frontend (.env.local)
NEXT_PUBLIC_API_URL=http://localhost:5203
```

## Development Guidelines

### Code Style
- Use C# 13 features (required, init, records)
- Follow Onion Architecture principles
- Keep DTOs in Application layer
- Keep entities in Core layer
- Use async/await for all I/O operations

### Testing
- Write E2E tests for all user flows
- Test both French and English locales
- Test all device types
- Test permission matrix

### Git Workflow
- Main branch: `main`
- Feature branches: `feature/description`
- Always run tests before committing
- Use conventional commits

### Security
- Never commit secrets
- Use Azure Key Vault for production
- Validate all user inputs
- Use parameterized queries (EF Core handles this)
- Hash passwords with BCrypt

## Future Improvements

### Immediate Priorities
1. Fix all 47 failing E2E tests
2. Update tests to match new onboarding flow
3. Test single house auto-redirect
4. Verify all locale prefixes work correctly

### Feature Backlog
1. Email notifications for upcoming maintenance
2. File attachments for maintenance logs
3. Stripe integration for premium subscriptions
4. Multi-house selection (if more than 1)
5. Export maintenance history to PDF/CSV

### Technical Debt
1. Set up backend code generation from OpenAPI (currently manual)
2. Add more comprehensive error handling
3. Implement retry logic for API calls
4. Add loading skeletons instead of "Loading..." text
5. Implement optimistic UI updates

## Contact & Resources

- **GitHub**: https://github.com/yourusername/ProjectAI
- **Documentation**: See `README.md`, `INSTALLATION_GUIDE.md`
- **Architecture Plan**: `analyse_technique/implementation_plan.md`
- **User Flows**: `analyse_technique/user_flows.md`
- **Wireframes**: `analyse_technique/wireframes/`

---

**Note**: This is a living document. Update it whenever significant changes are made to the project.
