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
cd homoeodesk.website && npm install && npm run dev   # port 3200
```

Marketing site env vars (optional local `.env`):

```env
VITE_DEMO_TENANT_URL=http://localhost:3000
VITE_GLOBAL_API_URL=http://localhost:7100
VITE_CONTACT_EMAIL=hello@homoeodesk.com
VITE_SITE_URL=http://localhost:3200
```

Public conversion endpoints on global API:

- `POST /api/public/trial-requests`
- `POST /api/public/register`

After pulling SQL changes, redeploy the global database so `TrialRequests` exists:

```powershell
.\scripts\deploy-databases.ps1 -GlobalOnly
```


## 4. Connection strings

Edit `appsettings.json` in each API project. Default user: `HomoeoDeskUser` / `HomoeoDesk@123`.

## 5. CI/CD

See [docs/cicd/README.md](cicd/README.md). Manifests live in `homoeodesk.azure/deployment/`.
