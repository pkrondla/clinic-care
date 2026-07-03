# HomoeoDesk

Multi-tenant homoeopathy clinic management SaaS.

## Structure

```
homoeodesk.global/     Control plane (tenant registry, global medicines, platform admin)
homoeodesk.tenant/     Per-tenant clinic operations (patients, appointments, billing, etc.)
homoeodesk.azure/      Bicep IaC and deployment manifests
homoeodesk.website/    Marketing landing site
homoeodesk.shared/     Shared API hosting extensions (CORS, Key Vault, App Insights)
```

## Quick start

### Backend

```powershell
dotnet build HomoeoDesk.sln
dotnet run --project homoeodesk.global/homoeodesk.global.api
dotnet run --project homoeodesk.tenant/homoeodesk.tenant.api
```

### Frontend

```powershell
cd homoeodesk.global/homoeodesk.global.web && npm install && npm run dev
cd homoeodesk.tenant/homoeodesk.tenant.web && npm install && npm run dev
```

### Database

Schema is owned by SSDT projects under `homoeodesk.*.database/`. See [DATABASE_PROJECTS.md](DATABASE_PROJECTS.md).

## Architecture

- [ADR-001: Slice boundaries](docs/architecture/adr-001-modular-monolith-boundaries.md)
- [ADR-002: Minimal APIs](docs/architecture/adr-002-minimal-apis-over-controllers.md)
- [CI/CD](docs/cicd/README.md)

## Ports (local dev)

| App | Port |
|-----|------|
| Global API | 7100 |
| Tenant API | 7000 |
| Global Web | 3001 |
| Tenant Web | 3000 |
