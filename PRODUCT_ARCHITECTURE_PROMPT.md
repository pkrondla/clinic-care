# Apply ChitStack Multi-Tenant Architecture

Use this document as a **Cursor/AI bootstrap prompt** when starting a new SaaS product. Copy it into the new project's repository root, fill in the **Product Context** table below, then paste the entire file (or reference it with `@PRODUCT_ARCHITECTURE_PROMPT.md`) as your first agent message.

**Reference implementation:** [ChitStack](https://github.com/your-org/chit-stack) — a production multi-tenant .NET + React platform with strict global/tenant slice boundaries, database-per-tenant Azure stamps, SSDT-owned schema, and manifest-driven CI/CD.

---

## Product Context (fill in before running)

| Placeholder | Your value |
|-------------|------------|
| `{ProductName}` | e.g. ClinicCare, InventoryPro |
| `{productprefix}` | lowercase kebab, e.g. `cliniccare`, `inventorypro` |
| `{ProductNamespace}` | PascalCase root namespace, e.g. `ClinicCare` |
| `{domain}` | e.g. healthcare, inventory, logistics |
| `{primaryTenantFeatures}` | bullet list of tenant-slice business capabilities |
| `{globalFeatures}` | tenant registry, subscriptions, affiliates, platform admin, etc. |
| `{tenantRoles}` | e.g. Admin, Staff, Manager, Agent, Customer |
| `{azureResourcePrefix}` | short prefix for Azure resources, e.g. `tr-cc` |
| `{productDomain}` | public domain for tenant portals, e.g. `cliniccare.com` |

---

## Instructions for the AI Agent

You are bootstrapping a new SaaS product using the **ChitStack modular monolith architecture** as the reference. Follow its patterns exactly unless this document specifies a product-specific variation.

**Your task:** Scaffold `{ProductName}` following this architecture. Start with Phase 1, confirm the solution structure with the user, then proceed through Phases 2–5. Adapt domain entities and feature areas to `{domain}` / `{primaryTenantFeatures}`, but preserve all structural, naming, multi-tenant, and deployment patterns from ChitStack.

Ask the user to confirm `{ProductName}`, `{productprefix}`, and `{primaryTenantFeatures}` before generating domain-specific entities.

---

## Core Architecture Principles

1. **Modular monolith with two vertical slices** — never merge global and tenant into one backend.
2. **Database-per-tenant in production** — each tenant gets its own Azure stamp (API + web + SQL).
3. **Global control plane** — one global stamp per environment for tenant registry, orders, subscriptions, platform users.
4. **Strict slice boundaries (ADR-001)** — global ↔ tenant projects must NEVER reference each other.
5. **SSDT DACPAC owns schema** — EF Core is runtime access only; no `Database.Migrate()` at deploy.
6. **API projects are composition roots** — controllers dispatch to MediatR; no business logic in controllers.
7. **No repository layer** — handlers inject `IApplicationDbContext` / `IGlobalDbContext` directly.

---

## Required Repository Layout

Create this top-level structure:

```
{productprefix}/
├── {ProductName}.sln
├── README.md
├── DATABASE_PROJECTS.md
├── docs/
│   ├── README.md
│   ├── getting-started.md
│   ├── security.md
│   ├── architecture/
│   │   ├── overview.md
│   │   ├── adr-001-modular-monolith-boundaries.md
│   │   ├── migrations-separation.md
│   │   └── baseline-smoke-matrix.md
│   ├── cicd/
│   │   ├── README.md
│   │   ├── architecture.md
│   │   └── onboarding.md
│   └── deployment/
│       └── azure-setup.md
│
├── {productprefix}.global/                    # Control plane slice
│   ├── {productprefix}.global.api/            # ASP.NET Core Web API (composition root)
│   ├── {productprefix}.global.application/    # MediatR CQRS handlers
│   ├── {productprefix}.global.domain/         # GlobalTenant, GlobalUser, Orders, etc.
│   ├── {productprefix}.global.infrastructure/ # GlobalDbContext, token/email services
│   ├── {productprefix}.global.database/       # SSDT: RunOnce/, Seeds/, Versioned/, PostDeploy.sql
│   └── {productprefix}.global.web/            # React + TypeScript admin portal
│
├── {productprefix}.tenant/                    # Per-tenant business slice
│   ├── {productprefix}.tenant.api/            # ASP.NET Core Web API + middleware + SignalR hubs
│   ├── {productprefix}.tenant.application/    # MediatR CQRS handlers
│   ├── {productprefix}.tenant.domain/         # Business entities
│   ├── {productprefix}.tenant.infrastructure/ # TenantDbContext, services, seeding
│   ├── {productprefix}.tenant.database/       # SSDT: RunOnce/, Seeds/, Versioned/, PostDeploy.sql
│   └── {productprefix}.tenant.web/            # React operations + customer/agent portals
│
├── {productprefix}.azure/                     # IaC + deployment
│   ├── iac/
│   │   ├── main.bicep
│   │   ├── env/                               # dev, uat, qa, prod parameters
│   │   └── modules/
│   │       ├── global-stamp/
│   │       ├── tenant-stamp/
│   │       ├── website-stamp/
│   │       ├── app-service/
│   │       ├── sql/
│   │       ├── static-web/
│   │       ├── key-vault/
│   │       └── observability/
│   └── deployment/
│       ├── global.json
│       ├── tenants.json
│       ├── website.json
│       ├── policies/
│       │   └── sku-policy.json
│       └── scripts/
│           ├── configure-stamp-app.ps1
│           └── register-global-tenant.ps1
│
├── {productprefix}.website/                   # Marketing/landing site (Vite + React)
├── .github/
│   └── workflows/
│       ├── ci.yml
│       ├── deploy-infra.yml
│       ├── provision-global.yml
│       ├── provision-tenant.yml
│       └── provision-website.yml
└── .vscode/                                   # Launch configs (optional)
```

### Solution structure (Visual Studio)

Open `{ProductName}.sln` to work across all .NET projects. Organize solution folders:

```
Solution Items/
  docs/
Global/
  {productprefix}.global.api
  {productprefix}.global.application
  {productprefix}.global.domain
  {productprefix}.global.infrastructure
  {productprefix}.global.database
Tenant/
  {productprefix}.tenant.api
  {productprefix}.tenant.application
  {productprefix}.tenant.domain
  {productprefix}.tenant.infrastructure
  {productprefix}.tenant.database
Azure/
  (IaC lives outside .sln or as solution folder reference)
```

---

## Backend Layer Structure (both slices)

Each slice follows Clean Architecture with four projects plus one database project:

```
*.api
  ├── Controllers/          # Thin controllers → IMediator.Send()
  ├── Middleware/           # TenantMiddleware, BranchMiddleware, ExceptionMiddleware
  ├── Hubs/                 # SignalR hubs (tenant slice)
  ├── Services/             # API-specific adapters only (e.g. notification bridges)
  └── Program.cs            # Composition root, DI, pipeline

*.application
  ├── Features/{Area}/
  │   ├── Commands/{Action}/
  │   │   ├── {Action}Command.cs       # IRequest<Result<T>>
  │   │   ├── {Action}Validator.cs     # FluentValidation (same file or adjacent)
  │   │   └── Handler in same file     # IRequestHandler
  │   └── Queries/{Action}/
  │       ├── {Action}Query.cs
  │       └── {Action}Handler.cs + DTOs
  ├── Common/
  │   ├── Interfaces/       # IApplicationDbContext, ITenantService, IBranchService
  │   ├── Models/           # Result<T>, PagedResult<T>
  │   └── Behaviours/       # MediatR pipeline behaviors
  └── DependencyInjection.cs

*.domain
  ├── Common/
  │   ├── BaseEntity.cs     # Id, CreatedAt, UpdatedAt, IsActive
  │   ├── TenantEntity.cs   # + TenantId
  │   └── BranchEntity.cs   # + optional BranchId (tenant slice)
  ├── Entities/
  └── Enums/

*.infrastructure
  ├── Data/
  │   └── {Global|Tenant}DbContext.cs
  ├── Services/             # TenantService, TokenService, EmailService
  ├── Configuration/        # TenantStampOptions
  ├── Jobs/                 # Hangfire workers
  └── DependencyInjection.cs

*.database (SSDT)
  ├── RunOnce/
  ├── Seeds/
  ├── Versioned/
  └── PostDeploy.sql
```

### Dependency rules (enforce in CI / architecture tests)

```
✅ {productprefix}.tenant.api → tenant.application → tenant.domain
✅ {productprefix}.tenant.infrastructure → tenant.application + tenant.domain
✅ {productprefix}.global.api → global.application → global.domain
✅ {productprefix}.global.infrastructure → global.application + global.domain

❌ tenant → global (forbidden)
❌ global → tenant (forbidden)
❌ shared/kernel → global or tenant (forbidden unless explicitly designed as contracts-only)
❌ Business logic in API controllers (forbidden)
```

### ADR-001: Modular Monolith Slice Boundaries

Document this in `docs/architecture/adr-001-modular-monolith-boundaries.md`:

1. Tenant slice must not reference global slice projects.
2. Global slice must not reference tenant slice projects.
3. Shared projects must never depend on global or tenant projects.
4. API projects are composition roots only — no business logic.
5. Short-term duplication is allowed when it preserves boundaries during extraction.

---

## Multi-Tenant Architecture

### High-level topology

```
┌──────────────────────────────────────────────────────────────────┐
│                        GitHub Repository                          │
│  {productprefix}.global.*  │  {productprefix}.tenant.*  │  .azure │
└──────────────────────────────────────────────────────────────────┘
                                │
                    CI (automatic) │ CD (manual)
                                ▼
┌──────────────────────────────────────────────────────────────────┐
│                         Azure (per stamp)                         │
│                                                                   │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────────────┐   │
│  │ Static Web  │───►│ App Service │───►│ Azure SQL Database  │   │
│  │   (React)   │    │  (.NET API) │    │                     │   │
│  └─────────────┘    └─────────────┘    └─────────────────────┘   │
│         │                  │                      │               │
│         └──────────────────┴──────────────────────┘               │
│                    Key Vault · App Insights · Log Analytics       │
└──────────────────────────────────────────────────────────────────┘
```

### Two "Tenant" concepts

| Location | Entity | Purpose |
|----------|--------|---------|
| **Global DB** | `GlobalTenant` | Registry: subdomain, connection string, subscription status, trial dates, provisioning metadata |
| **Tenant DB** | `Tenant` | Local org record with `TenantKey`; `Id` must match global `tenantId` |

### Isolation model

| Environment | Model |
|-------------|-------|
| **Production (Azure)** | One stamp per tenant — dedicated resource group, SQL database, App Service API, Static Web App |
| **Local dev** | Shared tenant DB; multiple logical tenants via JWT `TenantId` claim or `X-Tenant-Id` header |

Even with separate databases, tenant entities carry `TenantId`. EF query filters enforce it; `SaveChangesAsync` auto-sets `TenantId` on new `TenantEntity` rows.

### Tenant resolution (`ITenantService`) — priority order

1. **Fixed-tenant mode** (`TenantStamp:EnableFixedTenant=true`) → always use `FixedTenantId` (Azure production)
2. JWT claim **`TenantId`**
3. HTTP header **`X-Tenant-Id`** (dev/fallback)
4. Default to `1`

### Fixed-tenant configuration (Azure)

Each tenant API stamp serves exactly one tenant in production:

```json
"TenantStamp": {
  "EnableFixedTenant": true,
  "FixedTenantId": 2,
  "FixedTenantConnectionString": "<stamp-specific SQL connection>"
}
```

Set by deployment script `configure-stamp-app.ps1` from `tenants.json`.

`Program.cs` selects the connection string:

```csharp
var tenantStampOptions = builder.Configuration
    .GetSection(TenantStampOptions.SectionName)
    .Get<TenantStampOptions>() ?? new TenantStampOptions();

var tenantDbConnection = string.IsNullOrWhiteSpace(tenantStampOptions.FixedTenantConnectionString)
    ? builder.Configuration.GetConnectionString("TenantDBConnection")
    : tenantStampOptions.FixedTenantConnectionString;
```

### Branch scoping (tenant slice — second isolation dimension)

For organizations with multiple branches/locations:

- Entity hierarchy: `BaseEntity` → `TenantEntity` → `BranchEntity` (optional `BranchId`)
- EF Core global query filters on `TenantId` + optional `BranchId`
- Frontend sends **`X-Branch-Id`** header from persisted active branch
- `BranchMiddleware` sets scoped `IBranchService`
- Users may be assigned to specific branches via a join table (e.g. `UserBranches`)

Example query filter pattern:

```csharp
builder.Entity<{Entity}>().HasQueryFilter(x =>
    x.TenantId == _tenantService.TenantId &&
    (_branchService.BranchId == null || x.BranchId == null || x.BranchId == _branchService.BranchId));
```

### Tenant onboarding flows

| Flow | Behavior |
|------|----------|
| **Trial registration** | Create `GlobalTenant` with `SubscriptionStatus=DemoAccess`; seed user into shared demo tenant DB |
| **Paid subscription** | Order → payment → provisioning sets subdomain + connection string → CD provisions dedicated stamp |
| **Portal URLs** | `{tenantKey}.{productDomain}` after provisioning |

### GlobalTenant entity (control plane)

```csharp
public class GlobalTenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = "DemoAccess";
    public DateTime TrialStartDate { get; set; } = DateTime.UtcNow;
    public DateTime TrialEndDate { get; set; } = DateTime.UtcNow.AddDays(30);
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public DateTime? ProvisionedAt { get; set; }

    public bool IsTrialExpired =>
        SubscriptionStatus == "DemoAccess" && TrialEndDate < DateTime.UtcNow;
}
```

### Deployment manifest (`tenants.json`)

```json
{
  "defaults": {
    "environment": "prod",
    "profile": "nonprod-ultra-low",
    "region": "centralindia",
    "azureResourcePrefix": "{azureResourcePrefix}"
  },
  "profiles": {
    "nonprod-ultra-low": {
      "staticWebSku": "Free",
      "apiPlanSku": "F1",
      "sqlSku": "Basic"
    },
    "prod-min-safe": {
      "staticWebSku": "Free",
      "apiPlanSku": "B1",
      "sqlSku": "Basic"
    }
  },
  "tenants": [
    {
      "tenantKey": "demo",
      "tenantId": 1,
      "displayName": "Demo",
      "subdomain": "demo",
      "enabled": true,
      "profile": "nonprod-ultra-low",
      "environments": {
        "prod": { "enabled": true },
        "dev": { "enabled": false },
        "uat": { "enabled": false },
        "qa": { "enabled": false }
      },
      "overrides": {}
    }
  ]
}
```

### Azure resource naming

| Resource | Pattern |
|----------|---------|
| Global resource group | `rg-{productprefix}-{environment}-global` |
| Tenant resource group | `rg-{productprefix}-{environment}-{tenantKey}` |
| Global SQL database | `sqldb-{productprefix}-{environment}-global` |
| Tenant SQL database | `sqldb-{azureResourcePrefix}-{environment}-{tenantKey}` |
| API app | `api-{azureResourcePrefix}-{environment}-{tenantKey\|global}` |

---

## Database Architecture (SSDT DACPAC)

### Two database projects — schema source of truth

| Project | Purpose |
|---------|---------|
| `{productprefix}.global.database` | Control-plane schema |
| `{productprefix}.tenant.database` | Per-tenant business schema |

EF Core is used for **runtime data access only**. Schema changes are applied via **DACPAC publish** in CD pipelines, not EF migrations at deploy time.

### Folder conventions (both projects)

| Folder | Purpose |
|--------|---------|
| `RunOnce/` | Baseline schema — numbered `001.{seq}.{ObjectName}.sql` |
| `Seeds/` | Idempotent reference data via `PostDeploy.sql` |
| `Versioned/` | Manual release upgrade scripts (not in build model) |
| `PostDeploy.sql` | Ordered seed entrypoint |

### Authoring rules

- Add new schema objects under `RunOnce/`.
- Keep one primary object per script where practical.
- Required application defaults must live in SQL seed assets, not only in C# startup code.
- Seed scripts must be safe to execute more than once (`IF NOT EXISTS` / guarded inserts).
- Environment-specific credentials must not be hard-coded in seed scripts.
- Use `Versioned/` for release migrations, backfills, and one-off upgrade logic.
- When a versioned change becomes part of the long-term baseline, fold it into `RunOnce/`.

### Tenant seed alignment with global registry

Tenant default seed must accept `/v:TenantId=<n>` at deploy so local `Tenants.Id` aligns with global `tenants.json` `tenantId`:

```sql
-- $(TenantId) is passed from sqlpackage at deploy time via /v:TenantId=<n>
-- It must match the Id in the global Tenants table.
DECLARE @TenantId INT = $(TenantId);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Tenants] WHERE [Id] = @TenantId)
BEGIN
    SET IDENTITY_INSERT [dbo].[Tenants] ON;
    INSERT INTO [dbo].[Tenants] (
        [Id],
        [TenantKey],
        [Name],
        [SubscriptionStatus],
        [CreatedBy]
    )
    VALUES (
        @TenantId,
        N'default',
        N'Default Tenant',
        N'Active',
        N'System'
    );
    SET IDENTITY_INSERT [dbo].[Tenants] OFF;
END;
GO
```

### Deploy order

1. CI builds both DACPAC artifacts once.
2. Deploy global DACPAC once per target environment.
3. Deploy tenant DACPAC once per tenant stamp per environment with `/v:TenantId=<n>`.
4. Capture `SqlPackage` publish output as build artifacts for audit.

Recommended `SqlPackage` usage:

- Generate deploy report first (`/Action:DeployReport`) for review.
- Run publish with explicit `TargetServerName` and `TargetDatabaseName`.
- Block destructive changes by default unless approved change window exists.

### EF Core decommission checklist

Do not remove EF migration ownership until all of the following are true:

1. A brand-new global database can be created from `{productprefix}.global.database` alone.
2. A brand-new tenant database can be created from `{productprefix}.tenant.database` alone.
3. Required baseline/reference data exists after SQL project publish plus `PostDeploy.sql`.
4. Existing databases can be upgraded safely using database-project publish/versioned scripts.
5. No required schema creation is left in `Program.cs` or C# startup seeders used only for schema.

Only after those checks pass, remove `Database.Migrate()`, `EnsureCreated()`, and EF migration files used only for schema ownership.

---

## Technology Stack

### Backend (both slices)

| Package | Version (reference) | Purpose |
|---------|---------------------|---------|
| .NET | 9 | Runtime |
| MediatR | 14 | CQRS |
| FluentValidation | 12 | Input validation |
| AutoMapper | 16 | Entity ↔ DTO mapping |
| EF Core + SqlServer | 9 | Data access |
| Hangfire | 1.8 | Background jobs |
| SignalR | built-in | Real-time (tenant slice) |
| Serilog.AspNetCore | 8+ | Structured logging |
| Swashbuckle | 7+ | Swagger/OpenAPI |
| QuestPDF | latest | PDF generation (if needed) |
| System.IdentityModel.Tokens.Jwt | 8+ | JWT |

### Frontend

| App | Stack |
|-----|-------|
| `{productprefix}.global.web` | React 19, **TypeScript**, Vite 7, Ant Design 6, TanStack Query 5, Zustand 5, React Router 7, Axios |
| `{productprefix}.tenant.web` | React 19, **JavaScript**, Vite 7, Ant Design 6, Tailwind CSS 4, TanStack Query 5, Zustand 5, React Router 7, Axios, PWA (vite-plugin-pwa) |
| `{productprefix}.website` | Vite 7 + React 19 marketing site |

Real-time: `@microsoft/signalr` client on tenant web.

### Cloud & CI/CD

- Azure App Service, Static Web Apps, Azure SQL, Key Vault, Application Insights, Log Analytics
- Bicep modules in `{productprefix}.azure/iac/`
- GitHub Actions: automated CI on PR; manual approval-gated stamp deployments

---

## Backend Implementation Patterns

### Result pattern

```csharp
public class Result<T>
{
    public bool Succeeded { get; private set; }
    public T? Data { get; private set; }
    public List<string> Errors { get; private set; } = new();

    public static Result<T> Success(T data) => new() { Succeeded = true, Data = data };
    public static Result<T> Failure(string error) => new() { Succeeded = false, Errors = { error } };
    public static Result<T> Failure(IEnumerable<string> errors) => new() { Succeeded = false, Errors = errors.ToList() };
}
```

### Base entities

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

public abstract class TenantEntity : BaseEntity
{
    public int TenantId { get; set; }
}

public abstract class BranchEntity : TenantEntity
{
    public int? BranchId { get; set; }
}
```

### CQRS command (single-file pattern)

```csharp
public class Create{Entity}Command : IRequest<Result<int>>
{
    public string Name { get; set; } = string.Empty;
}

public class Create{Entity}Validator : AbstractValidator<Create{Entity}Command>
{
    public Create{Entity}Validator() => RuleFor(v => v.Name).NotEmpty();
}

public class Create{Entity}Handler : IRequestHandler<Create{Entity}Command, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public Create{Entity}Handler(IApplicationDbContext context) => _context = context;

    public async Task<Result<int>> Handle(Create{Entity}Command request, CancellationToken ct)
    {
        var entity = new {Entity} { Name = request.Name };
        _context.{Entities}.Add(entity);
        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(entity.Id);
    }
}
```

### Controller (thin dispatch only)

```csharp
[Authorize(Roles = "Admin,Staff")]
[ApiController]
[Route("api/[controller]")]
public class {Entities}Controller : ControllerBase
{
    private readonly IMediator _mediator;

    public {Entities}Controller(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<Result<List<{Entity}Dto>>>> Get()
        => Ok(await _mediator.Send(new Get{Entities}Query()));

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<{Entity}Dto>>> GetById(int id)
    {
        var result = await _mediator.Send(new Get{Entity}ByIdQuery { Id = id });
        if (!result.Succeeded) return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<int>>> Create(Create{Entity}Command command)
        => Ok(await _mediator.Send(command));
}
```

### TenantService

```csharp
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TenantStampOptions _tenantStampOptions;

    public int TenantId
    {
        get
        {
            if (_tenantStampOptions.EnableFixedTenant)
                return _tenantStampOptions.FixedTenantId;

            var context = _httpContextAccessor.HttpContext;
            if (context == null) return 1;

            var tenantClaim = context.User.FindFirst("TenantId");
            if (tenantClaim != null && int.TryParse(tenantClaim.Value, out int tenantId))
                return tenantId;

            if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue)
                && int.TryParse(headerValue, out int headerId))
                return headerId;

            return 1;
        }
    }
}
```

### Tenant API middleware pipeline order

```
Exception handling (dev/prod)
→ Swagger (dev only)
→ CORS
→ TenantMiddleware
→ BranchMiddleware
→ Authentication
→ Authorization
→ Controllers
→ SignalR hubs
→ Hangfire dashboard (dev/admin)
```

### Hangfire queues

| Slice | Queues |
|-------|--------|
| Tenant | `tenant-default`, `tenant-notifications`, `tenant-maintenance` |
| Global | `global-maintenance` |

### JWT configuration

Include at minimum: `sub`, `email`, `role`, **`TenantId`**

SignalR: pass token via `?access_token=` query string on hub connections.

```csharp
options.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;
        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/{hubName}"))
            context.Token = accessToken;
        return Task.CompletedTask;
    }
};
```

### Application DI registration

```csharp
public static IServiceCollection Add{Slice}ApplicationServices(this IServiceCollection services)
{
    services.AddAutoMapper(Assembly.GetExecutingAssembly());
    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    return services;
}
```

### DbContext interface (no repository layer)

Handlers depend on:

- **Tenant slice:** `IApplicationDbContext` (implemented by `TenantDbContext`)
- **Global slice:** `IGlobalDbContext` (implemented by `GlobalDbContext`)

---

## Frontend Implementation Patterns

### Three independent web apps

| App | Purpose |
|-----|---------|
| `{productprefix}.global.web` | Platform admin: tenants, orders, catalog, affiliates, settings; public register/subscribe |
| `{productprefix}.tenant.web` | Tenant operations, customer portal, agent workflows |
| `{productprefix}.website` | Marketing/landing; links to demo and global portal |

### Tenant web — role-based routing

Roles: `{tenantRoles}`

```
/              → operations layout (admin/staff workflows)
/portal        → customer/end-user portal (separate layout)
/agent         → field/mobile workflows (if applicable)
/login         → authentication
/trial-expired → shown when API returns 402 TrialExpired
```

Use `ProtectedRoute` component guards by role from JWT.

### Global web routing

```
Public:
  /register
  /subscribe
  /orders/:orderRef

Admin:
  /tenants
  /admin/orders
  /admin/catalog
  /admin/settings

Affiliate (optional):
  /affiliate/*
```

### API client (tenant web)

```javascript
import axios from 'axios';
import { apiBaseUrl } from '../config/apiConfig';

const api = axios.create({
    baseURL: apiBaseUrl,
    headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    const activeBranchStr = localStorage.getItem('activeBranch');

    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }

    if (activeBranchStr) {
        try {
            const activeBranch = JSON.parse(activeBranchStr);
            if (activeBranch?.id) {
                config.headers['X-Branch-Id'] = activeBranch.id.toString();
            }
        } catch (e) {
            // Ignore parse errors
        }
    }
    return config;
});

api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 402 && error.response?.data?.error === 'TrialExpired') {
            localStorage.removeItem('token');
            localStorage.removeItem('activeBranch');
            localStorage.removeItem('authorizedBranches');
            window.location.href = '/trial-expired';
        }
        return Promise.reject(error);
    }
);

export default api;
```

### Environment variables

| Variable | App | Purpose |
|----------|-----|---------|
| `VITE_API_URL` | tenant.web | Tenant API base URL |
| `VITE_GLOBAL_API_URL` | tenant.web | Global API (subscription banners, orders) |
| `VITE_GLOBAL_PORTAL_URL` | tenant.web | Link to global admin portal |
| `VITE_API_URL` | global.web | Global API base URL |

Local: `.env.development`  
Azure: set in Static Web App configuration during CD

### State management

- **Zustand** — auth store (token, user, active branch, authorized branches)
- **TanStack Query** — all server state (queries + mutations with cache invalidation)
- **SignalR client** — real-time features in tenant web

### Theming

- Design tokens in `src/theme/tokens.js` per app
- Add `npm run lint:theme` script to prevent hardcoded hex colors in page components
- Document token usage in `THEMING.md`

### Auth store (Zustand + persist)

Persist token and user; branch selection in `localStorage.activeBranch`.

---

## Global Slice Responsibilities

Implement `{globalFeatures}` in `{productprefix}.global.*`:

| Area | Examples |
|------|----------|
| **Tenant registry** | `GlobalTenant` CRUD, subdomain uniqueness |
| **Registration** | Trial signup, demo portal access |
| **Orders & subscriptions** | Catalog, checkout, provisioning completion |
| **Global auth** | Platform admin users, JWT issuance |
| **Affiliates** (optional) | Referral codes, commissions, payouts |
| **Platform settings** | Trial duration, demo portal URL, admin email |

Global application features organized under `Features/{Area}/Commands|Queries/`.

Global controllers (starting set): `Auth`, `Tenants`, `Registration`, `Users`, `Orders`, `Catalog`, `Subscriptions`, `Affiliates`, `Settings`.

---

## Tenant Slice Responsibilities

Implement `{primaryTenantFeatures}` in `{productprefix}.tenant.*`.

Organize application features under `Features/{Area}/` matching domain areas.

**ChitStack reference areas** (adapt names to your domain):

| Area | Examples |
|------|----------|
| ChitGroups | Create group, launch group, list groups |
| Auctions | Schedule, start, place bid, close |
| Collections | Agent routes, submissions, reconciliation |
| Financials | Journal entries, penalties, reconciliation |
| Reporting | P&L, balance sheet, statutory forms |
| Members | Registration, portal access, nominees |
| Auth | OTP, JWT token issuance |
| Users | CRUD, branch assignment |
| Settings | Tenant configuration |
| CustomerPortal | End-user self-service |

Tenant controllers: one per domain area; keep thin — dispatch to MediatR only.

---

## Azure Infrastructure

### Stamp topology

```
Global stamp (1 per environment):
  rg-{productprefix}-{env}-global
  ├── Static Web App ({productprefix}.global.web)
  ├── App Service ({productprefix}.global.api)
  └── SQL Database (global)

Tenant stamp (1 per tenant per environment):
  rg-{productprefix}-{env}-{tenantKey}
  ├── Static Web App ({productprefix}.tenant.web)
  ├── App Service ({productprefix}.tenant.api) — fixed-tenant mode
  └── SQL Database (tenant)

Website stamp (1 per environment):
  Static Web App ({productprefix}.website)
```

### Bicep module dispatch

`{productprefix}.azure/iac/main.bicep` routes to stamp modules:

| Module | Resources |
|--------|-----------|
| `global-stamp` / `tenant-stamp` | Orchestrates per-stamp resources |
| `static-web` | Azure Static Web App |
| `app-service` | App Service Plan + API web app |
| `sql` | SQL Server + database |
| `sql-app-outbound-firewall` | Restrict SQL to App Service outbound IPs |
| `key-vault` | Secrets management |
| `observability` | Log Analytics + Application Insights |

Parameters (SKUs, environment, tenant key) come from deployment manifests resolved at CD time.

### CI/CD workflows

| Workflow | Trigger | Action |
|----------|---------|--------|
| `ci.yml` | PR / push to main | Build, test, validate manifests, build DACPACs |
| `deploy-infra.yml` | Manual | Deploy shared infra if needed |
| `provision-global.yml` | Manual | Deploy global stamp from `global.json` |
| `provision-tenant.yml` | Manual | Deploy tenant stamp from `tenants.json` |
| `provision-website.yml` | Manual | Deploy marketing site from `website.json` |

CI must validate `global.json`, `tenants.json`, and `policies/sku-policy.json` on every PR.

### Observability

- Application Insights per stamp (connection string applied post-deploy)
- Structured logging via Serilog
- Slice-tagged telemetry: `slice=global` vs `slice=tenant`

---

## Configuration Reference

| Setting | Slice | Purpose |
|---------|-------|---------|
| `ConnectionStrings:GlobalDBConnection` | Global API | Global DB |
| `ConnectionStrings:TenantDBConnection` | Tenant API | Tenant DB (local dev) |
| `TenantStamp:EnableFixedTenant` | Tenant API | Pin stamp to one tenant (Azure) |
| `TenantStamp:FixedTenantId` | Tenant API | Tenant ID for this stamp |
| `TenantStamp:FixedTenantConnectionString` | Tenant API | Stamp-specific SQL connection |
| `SeedOnStartup` | Tenant API | C# seeder (local only; `false` in Azure) |
| `Jwt:Key` | Both | JWT signing key |
| `Jwt:Issuer` | Both | JWT issuer |
| `Jwt:Audience` | Both | JWT audience |
| `Cors:AllowedOrigins` | Both | SWA origins (auto-set in CD) |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Azure | Telemetry |

**Local dev:** User Secrets / `appsettings.Development.json`  
**Azure:** App Service settings via `configure-stamp-app.ps1`; secrets in Key Vault

---

## Security & Authentication

- JWT Bearer tokens issued by global and tenant auth endpoints
- Role claims drive API `[Authorize(Roles = "...")]` and frontend route guards
- Trial expiry: return HTTP 402 with `{ "error": "TrialExpired" }` — frontend redirects to `/trial-expired`
- CORS: localhost origins (dev) + `*.azurestaticapps.net` (prod)
- SignalR connections authenticate via query-string token
- Tenant isolation enforced at: fixed-tenant connection (prod), JWT claim (dev), EF query filters (all)

---

## Initialization Checklist

### Phase 1 — Solution skeleton

- [ ] Create `{ProductName}.sln` with global + tenant project groups
- [ ] Create all 12 .NET projects (6 global + 6 tenant) with correct project references
- [ ] Write ADR-001 in `docs/architecture/adr-001-modular-monolith-boundaries.md`
- [ ] Implement base types: `BaseEntity`, `TenantEntity`, `BranchEntity`, `Result<T>`
- [ ] Add `.gitignore`, `LICENSE`, root `README.md`

### Phase 2 — Global slice

- [ ] `GlobalTenant`, `GlobalUser` domain entities
- [ ] `GlobalDbContext` + `IGlobalDbContext`
- [ ] Global application: Auth, Registration, Tenants features
- [ ] Global API: Auth + Tenants + Registration controllers
- [ ] Global infrastructure: token service, email service, DI
- [ ] Global SSDT project: RunOnce baseline tables + Seeds + PostDeploy.sql
- [ ] Global web: register, login, tenant list admin pages
- [ ] `global.json` deployment manifest

### Phase 3 — Tenant slice

- [ ] Core domain entities for `{primaryTenantFeatures}`
- [ ] `TenantDbContext` with tenant + branch query filters
- [ ] `TenantService`, `BranchMiddleware`, `TenantStampOptions`
- [ ] Auth feature with JWT including `TenantId` claim
- [ ] At least one full CRUD feature (command + query + validator + controller)
- [ ] Tenant SSDT project with parameterized default tenant seed (`$(TenantId)`)
- [ ] Tenant web: login, auth store, axios interceptors, role-based routing
- [ ] `tenants.json` deployment manifest with demo tenant entry

### Phase 4 — Infrastructure

- [ ] `{productprefix}.azure/iac/` Bicep modules (global-stamp, tenant-stamp, website-stamp)
- [ ] `global.json`, `tenants.json`, `website.json`, `policies/sku-policy.json`
- [ ] `configure-stamp-app.ps1` — sets fixed-tenant config + CORS + connection strings
- [ ] GitHub Actions `ci.yml` — build, test, validate manifests, build DACPACs
- [ ] Manual provision workflows for global, tenant, website stamps

### Phase 5 — Polish

- [ ] Hangfire: at least one recurring job per slice
- [ ] Serilog structured logging in both APIs
- [ ] Swagger/OpenAPI in development
- [ ] `docs/getting-started.md` with local dev setup (SQL, user secrets, npm scripts)
- [ ] `DATABASE_PROJECTS.md` authoring guide
- [ ] Theme tokens + `lint:theme` guard on tenant web
- [ ] SignalR hub + client for at least one real-time feature (if applicable)
- [ ] Baseline smoke matrix in `docs/architecture/baseline-smoke-matrix.md`

---

## Example Feature Implementation Walkthrough

### Backend: Create `{Entity}` end-to-end

1. **Domain entity** — `{productprefix}.tenant.domain/Entities/{Entity}.cs` (extends `TenantEntity` or `BranchEntity`)
2. **RunOnce script** — `{productprefix}.tenant.database/RunOnce/001.xx.{Entities}.sql`
3. **DbSet** — add to `TenantDbContext` + query filter if branch-scoped
4. **Command** — `Features/{Area}/Commands/Create{Entity}/Create{Entity}Command.cs`
5. **Query + DTO** — `Features/{Area}/Queries/Get{Entities}/Get{Entities}Query.cs`
6. **Controller** — `{productprefix}.tenant.api/Controllers/{Entities}Controller.cs`

### Frontend: `{Entity}` list page

1. **Page** — `{productprefix}.tenant.web/src/pages/{Area}/{Entity}List.jsx`
2. **API calls** — use shared `axios` instance with TanStack Query
3. **Route** — add to `App.jsx` with `ProtectedRoute` role guard
4. **Navigation** — add to layout sidebar

---

## What NOT to Do

| Anti-pattern | Why |
|--------------|-----|
| Minimal APIs | ChitStack uses controllers — match that pattern |
| Repository layer | Handlers use `IApplicationDbContext` directly |
| EF migrations at deploy | SSDT DACPAC owns schema |
| Global ↔ tenant project references | Violates ADR-001 slice boundaries |
| Business logic in controllers | Controllers dispatch to MediatR only |
| Single monolithic backend | Control plane and tenant ops are separate slices |
| Hardcoded tenant connection strings | Use fixed-tenant config in Azure; registry in global DB |
| Schema in `Program.cs` or C# seeders (prod) | Schema belongs in SSDT RunOnce scripts |
| Shared mutable state between slices | Communicate via HTTP APIs or provisioning metadata only |

---

## Naming Conventions

| Area | Convention |
|------|------------|
| Folders/projects | `{productprefix}.{global\|tenant}.{layer}` (lowercase) |
| C# namespaces | `{ProductNamespace}.Application.*`, `{ProductNamespace}.Domain.*`, `{ProductNamespace}.Infrastructure.{Slice}.*` |
| Features | `Features/{Area}/Commands/{Name}/{Name}Command.cs` |
| SQL RunOnce | `001.{seq}.{ObjectName}.sql` |
| SQL Seeds | `001.{seq}.{Description}.sql` |
| Azure resources | `{type}-{azureResourcePrefix}-{environment}-{tenantKey\|global}` |
| Manifest keys | `tenantKey`, `tenantId`, `subdomain`, `displayName`, `profile` |
| Global domain types | Prefix with `Global` — e.g. `GlobalTenant`, `GlobalUser` |
| Env vars (frontend) | `VITE_` prefix |

---

## Quick Start Prompt (copy this block into Cursor)

```
@PRODUCT_ARCHITECTURE_PROMPT.md

Product context:
- ProductName: {ProductName}
- productprefix: {productprefix}
- ProductNamespace: {ProductNamespace}
- domain: {domain}
- productDomain: {productDomain}
- azureResourcePrefix: {azureResourcePrefix}
- tenantRoles: {tenantRoles}
- primaryTenantFeatures:
  - (list your features)
- globalFeatures:
  - tenant registry, registration, orders, subscriptions, platform admin

Start with Phase 1 of the initialization checklist. Create the solution skeleton
with all projects, ADR-001, and base entities. Show me the planned folder tree
before writing domain-specific code.
```

---

## ChitStack Reference Files

When adapting patterns, these files in the ChitStack repository are the canonical examples:

| Pattern | Reference file |
|---------|----------------|
| Architecture overview | `docs/architecture/overview.md` |
| Slice boundaries ADR | `docs/architecture/adr-001-modular-monolith-boundaries.md` |
| Database project guide | `DATABASE_PROJECTS.md` |
| Tenant resolution | `chitstack.tenant/chitstack.tenant.infrastructure/Services/TenantService.cs` |
| Fixed-tenant config | `chitstack.tenant/chitstack.tenant.infrastructure/Configuration/TenantStampOptions.cs` |
| EF tenant/branch filters | `chitstack.tenant/chitstack.tenant.infrastructure/Data/TenantDbContext.cs` |
| Tenant API bootstrap | `chitstack.tenant/chitstack.tenant.api/Program.cs` |
| Global registry entity | `chitstack.global/chitstack.global.domain/Global/GlobalTenant.cs` |
| Tenant registration | `chitstack.global/chitstack.global.application/Features/Registration/Commands/RegisterTenant/RegisterTenantCommand.cs` |
| CD tenant manifest | `chitstack.azure/deployment/tenants.json` |
| Tenant DB seed w/ TenantId | `chitstack.tenant/chitstack.tenant.database/Seeds/001.01.DefaultTenant.sql` |
| Frontend branch header | `chitstack.tenant/chitstack.tenant.web/src/api/axios.js` |
| Role-based routing | `chitstack.tenant/chitstack.tenant.web/src/App.jsx` |

---

*Generated from ChitStack architecture. Copy this file to your new project root and customize the Product Context table before use.*
