# HomoeoDesk Database Projects

Schema is owned by **SSDT DACPAC** projects, not EF Core migrations.

## Projects

| Project | Database | Purpose |
|---------|----------|---------|
| `homoeodesk.global.database` | `HomoeoDesk_Global` | Control plane: tenants, subscriptions, global medicines |
| `homoeodesk.tenant.database` | `HomoeoDesk_{tenantKey}` | Per-tenant clinic operations |

## Folder conventions

| Folder | Purpose |
|--------|---------|
| `RunOnce/` | Baseline schema — `001.{seq}.{ObjectName}.sql` |
| `Seeds/` | Idempotent reference data |
| `Versioned/` | Release upgrade scripts (OrganizationId → TenantId, etc.) |
| `PostDeploy.sql` | Ordered seed entrypoint |

## Deploy order

1. CI builds both DACPAC artifacts
2. Publish global DACPAC once per environment
3. Publish tenant DACPAC per stamp with `/v:TenantId=<n>`

## Local development

Use `scripts/deploy-databases.ps1` from the repository root (RunOnce → Seeds → Versioned → PostDeploy):

```powershell
.\scripts\deploy-databases.ps1
```

Build DACPAC artifacts (requires `Microsoft.Build.Sql` SDK):

```powershell
.\scripts\build-dacpac.ps1
```

Connection strings in API `appsettings.Development.json`:
- Global: `HomoeoDesk_Global`
- Tenant: `HomoeoDesk_demo` (shared dev DB with JWT/header tenant resolution)
