# HomoeoDesk — local development

## Prerequisites

- .NET 9 SDK
- Node.js 20+
- SQL Server (local or Azure SQL)

## 1. Databases

```powershell
.\scripts\deploy-databases.ps1
# Or global only / tenant only:
.\scripts\deploy-databases.ps1 -GlobalOnly
.\scripts\deploy-databases.ps1 -TenantOnly -TenantId 1
```

Creates `HomoeoDesk_Global` and `HomoeoDesk_demo`.

## 2. Backend

```powershell
dotnet build HomoeoDesk.sln
dotnet run --project homoeodesk.global/homoeodesk.global.api   # port 7100
dotnet run --project homoeodesk.tenant/homoeodesk.tenant.api   # port 7000
```

## 3. Frontend

```powershell
cd homoeodesk.global/homoeodesk.global.web && npm install && npm run dev
cd homoeodesk.tenant/homoeodesk.tenant.web && npm install && npm run dev
cd homoeodesk.website && npm install && npm run dev
```

## 4. Connection strings

Edit `appsettings.json` in each API project. Default user: `HomoeoDeskUser` / `HomoeoDesk@123`.

## 5. CI/CD

See [docs/cicd/README.md](cicd/README.md). Manifests live in `homoeodesk.azure/deployment/`.
