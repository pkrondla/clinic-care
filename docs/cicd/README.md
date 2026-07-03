# HomoeoDesk CI/CD

## GitHub secrets

Configure per environment (`dev`, `uat`, `prod`):

| Secret | Purpose |
|--------|---------|
| `AZURE_CREDENTIALS` | Service principal JSON for `azure/login@v2` |

## Workflows

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| `ci.yml` | PR / push to main | Build .NET solution, frontends, DACPACs; validate manifests |
| `provision-global.yml` | Manual | Deploy global API + global.web stamp |
| `provision-tenant.yml` | Manual | Deploy tenant API + tenant.web from `tenants.json` |
| `provision-website.yml` | Manual | Deploy marketing site |
| `deploy-infra.yml` | Manual | Validate and deploy Bicep infrastructure |

## Manifests

- [global.json](../homoeodesk.azure/deployment/global.json)
- [tenants.json](../homoeodesk.azure/deployment/tenants.json)
- [website.json](../homoeodesk.azure/deployment/website.json)
- [sku-policy.json](../homoeodesk.azure/deployment/policies/sku-policy.json)

## Local build

```powershell
dotnet build HomoeoDesk.sln
dotnet build homoeodesk.global/homoeodesk.global.database/homoeodesk.global.database.sqlproj
dotnet build homoeodesk.tenant/homoeodesk.tenant.database/homoeodesk.tenant.database.sqlproj
```
