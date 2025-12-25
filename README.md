# HouseFlow - Home Maintenance Management Application

HouseFlow is a comprehensive home maintenance tracking application built with .NET 10, Aspire, and Next.js 14+. It allows users to manage their properties, devices, and maintenance schedules efficiently.

## 🏗️ Architecture

This project follows the **Onion Architecture** pattern:

- **Core**: Domain entities and business logic
- **Application**: Use cases, DTOs, and interfaces
- **Infrastructure**: Data access, EF Core, external services
- **API**: REST API controllers and configuration
- **Frontend**: Next.js 14+ application (TypeScript + Tailwind CSS)

## 🚀 Technology Stack

### Backend
- **.NET 10** with C# 13
- **ASP.NET Core** Web API
- **Entity Framework Core 10** with PostgreSQL
- **Aspire** for orchestration
- **NSwag** for OpenAPI/Swagger documentation
- **JWT** for authentication
- **BCrypt.Net** for password hashing

### Frontend
- **Next.js 14+** with App Router
- **TypeScript**
- **Tailwind CSS**
- **Auto-generated TypeScript client** from OpenAPI spec

### Testing
- **xUnit** for unit and integration tests
- **Moq** for mocking
- **FluentAssertions** for readable assertions
- **WebApplicationFactory** for integration testing

## 📋 Prerequisites

- **.NET 10 SDK** (Preview)
- **Node.js 20+** and npm
- **PostgreSQL 16+**
- **Docker** (optional, for containerized deployment)

## 🛠️ Installation & Setup

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/ProjectAI.git
cd ProjectAI
```

### 2. Setup the Database

Install PostgreSQL and create a database:

```bash
createdb houseflow
```

Update the connection string in `src/HouseFlow.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=houseflow;Username=postgres;Password=yourpassword"
  }
}
```

### 3. Apply Database Migrations

```bash
cd src/HouseFlow.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../HouseFlow.API
dotnet ef database update --startup-project ../HouseFlow.API
```

### 4. Run the Backend

```bash
cd src/HouseFlow.API
dotnet run
```

The API will be available at `https://localhost:5001` and `http://localhost:5000`.

OpenAPI documentation: `https://localhost:5001/swagger`

### 5. Setup the Frontend

```bash
cd src/frontend
npm install
npm run dev
```

The frontend will be available at `http://localhost:3000`.

## 🧪 Running Tests

### Unit Tests

```bash
dotnet test tests/HouseFlow.UnitTests
```

### Integration Tests

```bash
dotnet test tests/HouseFlow.IntegrationTests
```

### Run All Tests

```bash
dotnet test
```

## 📚 API Endpoints

### Authentication
- `POST /v1/auth/register` - Register a new user
- `POST /v1/auth/login` - Login and get JWT token

### Houses
- `GET /v1/houses` - Get all user's houses
- `POST /v1/houses` - Create a new house
- `GET /v1/houses/{houseId}` - Get house details
- `POST /v1/houses/{houseId}/members` - Invite a member

### Devices
- `GET /v1/houses/{houseId}/devices` - Get house devices
- `POST /v1/houses/{houseId}/devices` - Create a device
- `GET /v1/devices/{deviceId}` - Get device details

### Maintenance
- `GET /v1/devices/{deviceId}/maintenance-types` - Get maintenance types
- `POST /v1/devices/{deviceId}/maintenance-types` - Create maintenance type
- `POST /v1/maintenance-types/{typeId}/instances` - Log maintenance
- `GET /v1/devices/{deviceId}/maintenance-instances` - Get maintenance history

## 🔑 Environment Variables

Create `appsettings.Development.json` for local development:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=houseflow;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyForJWTTokenGeneration123456",
    "Issuer": "HouseFlowAPI",
    "Audience": "HouseFlowClient"
  }
}
```

⚠️ **Never commit real secrets to version control!**

## 📦 Project Structure

```
ProjectAI/
├── src/
│   ├── HouseFlow.Core/              # Domain entities
│   ├── HouseFlow.Application/       # DTOs, interfaces, services
│   ├── HouseFlow.Infrastructure/    # EF Core, database, implementations
│   ├── HouseFlow.API/              # API controllers, configuration
│   ├── HouseFlow.AppHost/          # Aspire orchestration
│   └── frontend/                   # Next.js application
├── tests/
│   ├── HouseFlow.UnitTests/        # Unit tests
│   └── HouseFlow.IntegrationTests/ # Integration tests
├── analyse/                        # Requirements documentation
└── analyse_technique/              # Technical specifications
```

## 🤖 AI Agent Maintenance Guide

See [AI_AGENT_GUIDE.md](./AI_AGENT_GUIDE.md) for detailed instructions on how to maintain and evolve this codebase using AI agents.

## 🔄 Database Migrations

### Create a Migration

```bash
cd src/HouseFlow.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../HouseFlow.API
```

### Apply Migrations

```bash
dotnet ef database update --startup-project ../HouseFlow.API
```

### Remove Last Migration

```bash
dotnet ef migrations remove --startup-project ../HouseFlow.API
```

## 📐 API-First Development Workflow

This project follows an **API-First (Contract-First)** approach where the OpenAPI specification is the source of truth.

### Modifying the API

**IMPORTANT**: Always follow this workflow when making API changes:

1. **Update the OpenAPI Specification**
   ```bash
   # Edit the OpenAPI spec
   code analyse_technique/openapi.yaml
   ```

2. **Regenerate Frontend Client**
   ```bash
   cd src/HouseFlow.Frontend
   npm run generate-client
   ```
   This generates TypeScript types and client code in `src/lib/api/generated/`

3. **Update Backend Code**
   - Manually update DTOs in `src/HouseFlow.Application/DTOs/` to match the spec
   - Update entities in `src/HouseFlow.Core/Entities/` if needed
   - Update services in `src/HouseFlow.Infrastructure/Services/`
   - The backend uses NSwag to serve OpenAPI docs at `/swagger`

4. **Run Tests**
   ```bash
   # Backend tests
   dotnet test

   # Frontend E2E tests
   cd src/HouseFlow.Frontend
   npm test
   ```

### Why API-First?

- **Single Source of Truth**: The OpenAPI spec (`openapi.yaml`) defines the contract
- **Type Safety**: Frontend gets auto-generated TypeScript types
- **Documentation**: API documentation stays in sync with code
- **Contract Testing**: Both frontend and backend conform to the same contract

### Configuration Files

- **OpenAPI Spec**: `analyse_technique/openapi.yaml`
- **Frontend Config**: `src/HouseFlow.Frontend/openapi-ts.config.ts`
- **Generated Client**: `src/HouseFlow.Frontend/src/lib/api/generated/`

## 🐳 Docker Deployment

```bash
docker-compose up -d
```

## 📝 Contributing

1. Create a feature branch
2. Make your changes
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## 📄 License

This project is private and proprietary.

## 👥 Team

Developed by the Antigravity AI team.
