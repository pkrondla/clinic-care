### Quick orientation for AI coding agents

This repository is a Modular-Monolith built with .NET 9 (backend) and React + Vite (frontend). The backend follows Vertical Slices + DDD + Minimal APIs and is organized into Application, Domain, Infrastructure and API layers. Use this file to find the most important patterns and concrete examples to edit or extend the project.

Key files to read before making changes
- `backend/ClinicCare.API/Program.cs` — application startup (Serilog, CORS, JWT, SignalR, tenant middleware, endpoint mapping, DB seed)
- `backend/ClinicCare.API/Modules/*` — Minimal API modules / vertical slices (e.g. `Appointments` module contains endpoints)
- `backend/ClinicCare.Application/DependencyInjection.cs` — registers AutoMapper, FluentValidation and MediatR behaviours (see TenantBehaviour)
- `backend/ClinicCare.Infrastructure/DependencyInjection.cs` — registers DB context, repositories and infrastructure services (ITenantService, ICurrentUserService, IPasswordHasher, ITokenService)
- `backend/ClinicCare.Infrastructure/Data` — `ApplicationDbContext` and repositories (repository pattern examples in `REPOSITORY_ARCHITECTURE.md`)

Big picture architecture (short)
- Modular Monolith: each feature lives in a module (endpoints, handlers, dto, validators) under the API/Modules folder.
- Vertical slices + CQRS: commands/queries handled via MediatR. Look for request/handler pairs in `Application/Features`.
- Repositories: Application defines interfaces; Infrastructure implements them. Use `IApplicationDbContext` or specific repositories (e.g. `IAppointmentRepository`).
- Multi-tenancy: tenant resolution happens in a middleware in `Program.cs` and uses `ITenantService`. Many handlers expect tenant context — avoid removing tenant resolution.
- Real-time: SignalR hub at `/queueHub` (class `ClinicCare.API.Hubs.QueueHub`) used for live queue updates.

Concrete patterns and examples to follow
- Add services: call `builder.Services.AddApplicationServices()` and `builder.Services.AddInfrastructureServices(Configuration)` — prefer existing extension methods over ad-hoc registrations.
- Register new repositories/services in `ClinicCare.Infrastructure/DependencyInjection.cs` and add interfaces in `ClinicCare.Application/Common/Interfaces`.
- Add MediatR handlers/validators in `ClinicCare.Application` and rely on assembly scanning (AutoMapper, Validators, MediatR are registered by assembly).
- Minimal API endpoints: use MapGroup + MapPost/MapGet etc. See `MINIMAL_API_ARCHITECTURE.md` and `Modules/Appointments/AppointmentsEndpoints.cs` for shape. Handlers should be invoked via `IMediator` rather than directly calling repositories from endpoints when possible.

Build / run / debug notes (developer workflows)
- Backend (dev):
  - From repository root: `dotnet run --project backend/ClinicCare.API` or use the provided VS Code/.sln tasks. Program performs DB seeding on startup.
  - Build solution: `dotnet build ClinicCare.sln`
  - Run tests (if added): use `dotnet test` in test projects.
- Frontend (dev):
  - `cd frontend` then `npm install` and `npm run dev` (Vite). Environment variables live in `.env` or `frontend/README.md` examples.

Important repository-specific conventions
- Use the existing DependencyInjection extension methods (`AddApplicationServices`, `AddInfrastructureServices`) instead of duplicating registrations.
- Prefer MediatR pipeline behaviours for cross-cutting concerns — TenantBehaviour is used to enforce tenant context.
- Minimal API endpoint files are the canonical place for wiring routes; keep business logic in Application handlers.
- DB seeding is executed on startup in `Program.cs` using `DatabaseSeeder.SeedAsync` — be cautious when editing startup code to not accidentally run destructive seeds.

Integration points & external dependencies
- SQL Server (connection string key: `DefaultConnection` in appsettings).
- JWT: settings read from `Jwt` section in `appsettings.*.json`.
- SignalR hub path: `/queueHub` — frontend expects this for real-time updates (see `frontend/src/hooks/useSignalR.ts`).
- External notification integrations (WhatsApp/Email/SMS) are feature-flagged in `appsettings` / `Features`.

When editing code, be explicit and conservative
- Preserve DI registrations and MediatR wiring. If you add a new handler or validator, it will be picked up by assembly scanning.
- When introducing new endpoints, add them under `ClinicCare.API/Modules/<Feature>` and call the mediator; follow existing MapGroup patterns.
- If touching tenant or auth code, ensure you run manual end-to-end checks (login, tenant header `X-Tenant-Subdomain` or subdomain) because multi-tenancy is sensitive.

Files that document patterns
- `REPOSITORY_ARCHITECTURE.md`, `MINIMAL_API_ARCHITECTURE.md`, and the root `README.md` contain canonical explanations and examples — reference them when unsure.

If something is missing or unclear
- Ask for the preferred approach (e.g., add caching vs. repository-level optimization) and point to the file you plan to change.

Next step for me: I can merge this into the repo — tell me if you want any phrasing changes or additional examples (e.g., a short snippet for adding a new MediatR handler).
