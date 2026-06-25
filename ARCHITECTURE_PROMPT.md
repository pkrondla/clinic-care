# Full-Stack Application Architecture Prompt

Use this prompt to create a new project following the same architecture, technologies, and patterns as the ClinicCare reference implementation.

## Project Overview

Create a **multi-tenant SaaS application** with the following characteristics:

### Core Requirements
- **Backend**: .NET 9.0 with Clean Architecture (CQRS pattern using MediatR)
- **Frontend**: React 19 with TypeScript, Vite, TanStack Router, React Query, Zustand
- **Database**: SQL Server (Azure SQL) with multi-tenant architecture (Global DB + Tenant DBs)
- **Real-time**: SignalR for WebSocket communication
- **Background Jobs**: Hangfire for scheduled/recurring tasks
- **Authentication**: JWT Bearer tokens with role-based authorization
- **API Style**: Minimal APIs with endpoint groups (module-based organization)

---

## Backend Architecture

### Project Structure (Clean Architecture)

```
Backend/
├── {ProjectName}.API/              # Presentation Layer
│   ├── Controllers/               # (Optional - prefer minimal APIs)
│   ├── Endpoints/                 # Minimal API endpoint registry
│   │   └── EndpointsRegistry.cs  # Central endpoint mapping
│   ├── Modules/                   # Feature-based endpoint modules
│   │   └── {Feature}/
│   │       └── {Feature}Endpoints.cs
│   ├── Middleware/               # Custom middleware
│   │   ├── ExceptionMiddleware.cs
│   │   └── TenantMiddleware.cs
│   ├── Hubs/                     # SignalR hubs
│   ├── Filters/                  # Action filters
│   ├── Services/                 # API-specific services
│   └── Program.cs                # Application entry point
│
├── {ProjectName}.Application/     # Application Layer (Business Logic)
│   ├── Features/                 # CQRS features
│   │   └── {Feature}/
│   │       ├── Commands/
│   │       │   └── {Action}/
│   │       │       ├── {Action}Command.cs
│   │       │       ├── {Action}Handler.cs
│   │       │       └── {Action}Validator.cs (FluentValidation)
│   │       ├── Queries/
│   │       │   └── {Action}/
│   │       │       ├── {Action}Query.cs
│   │       │       ├── {Action}Handler.cs
│   │       │       └── {Action}Dto.cs
│   │       └── Events/           # Domain events (optional)
│   ├── Common/
│   │   ├── Behaviours/           # MediatR pipeline behaviors
│   │   ├── Interfaces/           # Application contracts
│   │   ├── Mappings/             # AutoMapper profiles
│   │   ├── Models/               # DTOs, Result types
│   │   └── Services/             # Application services
│   └── DependencyInjection.cs
│
├── {ProjectName}.Domain/          # Domain Layer
│   ├── Entities/                 # Domain entities
│   ├── Enums/                    # Domain enumerations
│   ├── Common/
│   │   ├── BaseEntity.cs         # Base entity with audit fields
│   │   └── TenantEntity.cs       # Tenant-aware base entity
│   └── Modules/                   # Feature modules (optional)
│
└── {ProjectName}.Infrastructure/  # Infrastructure Layer
    ├── Data/
    │   ├── {Global}DbContext.cs  # Global database context
    │   ├── {Tenant}DbContext.cs  # Tenant database context
    │   └── Repositories/         # Repository implementations
    │       ├── Global/           # Global repositories
    │       └── Tenant/           # Tenant repositories
    ├── Services/                  # Infrastructure services
    ├── Jobs/                      # Hangfire background jobs
    ├── Migrations/                # EF Core migrations
    └── DependencyInjection.cs
```

### Key Technologies & Packages

#### API Layer
- **.NET 9.0** (Web SDK)
- **MediatR** (v13.0.0) - CQRS pattern
- **Microsoft.AspNetCore.Authentication.JwtBearer** (v9.0.0)
- **Microsoft.AspNetCore.SignalR** (v1.1.0)
- **Hangfire.Core/AspNetCore/SqlServer** (v1.8.17)
- **Serilog.AspNetCore** (v8.0.3) - Structured logging
- **Swashbuckle.AspNetCore** (v7.0.0) - Swagger/OpenAPI

#### Application Layer
- **MediatR** (v13.0.0)
- **FluentValidation** (v11.11.0) - Input validation
- **AutoMapper** (v12.0.1) - Object mapping
- **Microsoft.EntityFrameworkCore** (v9.0.8)

#### Infrastructure Layer
- **Microsoft.EntityFrameworkCore.SqlServer** (v9.0.0)
- **MailKit/MimeKit** (v4.8.0) - Email
- **QuestPDF** (v2025.7.4) - PDF generation
- **System.IdentityModel.Tokens.Jwt** (v8.1.2)

### Implementation Patterns

#### 1. CQRS with MediatR

**Command Pattern:**
```csharp
// Command
public class CreatePatientCommand : IRequest<Result<PatientDto>>
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    // ... other properties
}

// Handler
public class CreatePatientHandler : IRequestHandler<CreatePatientCommand, Result<PatientDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public CreatePatientHandler(
        IApplicationDbContext context,
        ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<PatientDto>> Handle(
        CreatePatientCommand request, 
        CancellationToken cancellationToken)
    {
        var organizationId = await _tenantService.GetOrganizationIdAsync();
        
        // Business logic here
        // Return Result<T>.Success(data) or Result<T>.Failure(message)
    }
}

// Validator (FluentValidation)
public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
        
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");
    }
}
```

**Query Pattern:**
```csharp
// Query
public class GetPatientsQuery : IRequest<Result<PagedResult<PatientDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}

// Handler
public class GetPatientsHandler : IRequestHandler<GetPatientsQuery, Result<PagedResult<PatientDto>>>
{
    // Implementation similar to command handler
}
```

#### 2. Result Pattern

```csharp
public class Result<T>
{
    public bool Succeeded { get; private set; }
    public T? Data { get; private set; }
    public List<string> Errors { get; private set; } = new();

    public static Result<T> Success(T data) => new() { Succeeded = true, Data = data };
    public static Result<T> Failure(string error) => new() { Succeeded = false, Errors = { error } };
    public static Result<T> Failure(List<string> errors) => new() { Succeeded = false, Errors = errors };
}
```

#### 3. Base Entities

```csharp
// Base entity with audit fields
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    public void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

// Tenant-aware entity
public abstract class TenantEntity : BaseEntity
{
    public int OrganizationId { get; set; }
}
```

#### 4. Minimal API Endpoints

```csharp
// Module-based endpoint organization
public static class PatientsEndpoints
{
    public static IEndpointRouteBuilder MapPatientsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/patients")
            .WithTags("Patients")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetPatients)
            .WithName("GetPatients")
            .WithSummary("Get all patients with filtering and pagination");

        group.MapPost("/", CreatePatient)
            .WithName("CreatePatient")
            .WithSummary("Create a new patient");

        return app;
    }

    private static async Task<IResult> GetPatients(
        [AsParameters] GetPatientsQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        
        if (!result.Succeeded)
        {
            return Results.BadRequest(new { 
                message = "Failed to retrieve patients", 
                errors = result.Errors 
            });
        }

        return Results.Ok(new { 
            message = "Patients retrieved successfully", 
            data = result.Data 
        });
    }
}

// Central registry
public static class EndpointsRegistry
{
    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPatientsEndpoints();
        app.MapAppointmentsEndpoints();
        // ... other endpoints
        return app;
    }
}
```

#### 5. Multi-Tenant Architecture

**Tenant Resolution Middleware:**
```csharp
// TenantMiddleware resolves organization from subdomain
app.Use(async (context, next) =>
{
    var tenantService = context.RequestServices.GetRequiredService<ITenantService>();
    var organizationId = await tenantService.GetOrganizationIdAsync();
    await next();
});
```

**Database Context Factory:**
```csharp
// Dynamic tenant database connection
services.AddScoped<TenantDbContext>(serviceProvider =>
{
    var tenantService = serviceProvider.GetRequiredService<ITenantService>();
    var subdomain = tenantService.Subdomain ?? "demo";
    var connectionString = baseConnectionString.Replace("{TenantId}", subdomain);
    // Create context with connection string
});
```

**Global Query Filters:**
```csharp
// Automatic tenant filtering in EF Core
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.Entity<Patient>()
        .HasQueryFilter(p => p.OrganizationId == _tenantService.OrganizationId);
}
```

#### 6. Dependency Injection Setup

**Application Layer:**
```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TenantBehaviour<,>));
        });
        return services;
    }
}
```

**Infrastructure Layer:**
```csharp
public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services, 
    IConfiguration configuration)
{
    // Global DB Context
    services.AddDbContext<GlobalDbContext>(options =>
        options.UseSqlServer(globalConnectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(maxRetryCount: 5);
            sqlOptions.CommandTimeout(30);
        }));

    // Tenant DB Context (factory pattern)
    services.AddScoped<TenantDbContext>(/* factory implementation */);

    // Repositories
    services.AddScoped<IPatientRepository, PatientRepository>();
    
    // Services
    services.AddScoped<ITenantService, TenantService>();
    services.AddScoped<IPasswordHasher, PasswordHasher>();
    services.AddScoped<ITokenService, TokenService>();
    
    return services;
}
```

#### 7. Program.cs Configuration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// Controllers & JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// SignalR
builder.Services.AddSignalR();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* JWT config */ });

// Application & Infrastructure
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Hangfire
builder.Services.AddHangfire(configuration => configuration
    .UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();

var app = builder.Build();

// Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");
app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapAllEndpoints();
app.MapHub<QueueHub>("/queueHub");
app.UseHangfireDashboard("/hangfire");

app.Run();
```

---

## Frontend Architecture

### Project Structure

```
frontend/
├── src/
│   ├── apps/                      # Application shells
│   │   ├── global/               # Global admin app
│   │   └── tenant/               # Tenant app
│   ├── components/               # Reusable components
│   │   ├── shared/              # Shared components
│   │   └── auth/                # Auth components
│   ├── core/
│   │   ├── config/              # Configuration
│   │   ├── hooks/               # Custom hooks
│   │   │   └── queries/         # React Query hooks
│   │   ├── providers/           # Context providers
│   │   ├── services/            # API services
│   │   ├── stores/              # Zustand stores
│   │   └── types/               # TypeScript types
│   ├── types/                   # Global types
│   ├── App.tsx                  # Root component
│   └── main.tsx                 # Entry point
├── public/
│   └── config.js                # Runtime configuration
├── package.json
├── vite.config.ts
└── tsconfig.json
```

### Key Technologies & Packages

#### Core Dependencies
- **React** (^19.1.1)
- **TypeScript** (~5.8.3)
- **Vite** (^7.1.2) - Build tool
- **@tanstack/react-router** (^1.131.13) - Routing
- **@tanstack/react-query** (^5.85.3) - Data fetching
- **@tanstack/react-table** (^8.21.3) - Tables
- **zustand** (^5.0.7) - State management
- **axios** (^1.11.0) - HTTP client
- **antd** (^5.27.0) - UI component library
- **@microsoft/signalr** (^9.0.6) - WebSocket client
- **zod** (^4.0.17) - Schema validation
- **react-hot-toast** (^2.5.2) - Notifications

#### Dev Dependencies
- **@vitejs/plugin-react** (^5.0.0)
- **vite-plugin-pwa** (^1.0.2) - PWA support
- **typescript-eslint** (^8.39.1)
- **vitest** (^3.2.4) - Testing

### Implementation Patterns

#### 1. API Client Setup

```typescript
// core/services/apiClient.ts
import axios, { AxiosInstance } from 'axios'
import { useAuthStore } from '../stores/authStore'

const apiClient: AxiosInstance = axios.create({
  baseURL: API_URL,
  timeout: 30000,
  headers: { 'Content-Type': 'application/json' }
})

// Request interceptor
apiClient.interceptors.request.use((config) => {
  const authState = useAuthStore.getState()
  if (authState.token) {
    config.headers.Authorization = `Bearer ${authState.token}`
  }
  return config
})

// Response interceptor
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().logout()
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export const api = {
  get: <T>(url: string) => apiClient.get(url).then(r => r.data),
  post: <T>(url: string, data?: unknown) => apiClient.post(url, data).then(r => r.data),
  put: <T>(url: string, data?: unknown) => apiClient.put(url, data).then(r => r.data),
  delete: <T>(url: string) => apiClient.delete(url).then(r => r.data)
}
```

#### 2. Service Layer

```typescript
// core/services/patientService.ts
import { api } from './apiClient'
import type { Patient, PatientDto, PagedResult } from '../types/patient'

export const patientService = {
  getAll: async (params?: {
    pageNumber?: number
    pageSize?: number
    searchTerm?: string
  }): Promise<{ message: string; data: PagedResult<PatientDto> }> => {
    return api.get('/api/patients', { params })
  },

  getById: async (id: number): Promise<{ message: string; data: PatientDto }> => {
    return api.get(`/api/patients/${id}`)
  },

  create: async (patient: Partial<Patient>): Promise<{ message: string; data: PatientDto }> => {
    return api.post('/api/patients', patient)
  },

  update: async (id: number, patient: Partial<Patient>): Promise<{ message: string; data: PatientDto }> => {
    return api.put(`/api/patients/${id}`, patient)
  },

  delete: async (id: number): Promise<{ message: string }> => {
    return api.delete(`/api/patients/${id}`)
  }
}
```

#### 3. React Query Hooks

```typescript
// core/hooks/queries/usePatients.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { patientService } from '@services/patientService'
import toast from 'react-hot-toast'

export const usePatients = (params?: {
  pageNumber?: number
  pageSize?: number
  searchTerm?: string
}) => {
  return useQuery({
    queryKey: ['patients', params],
    queryFn: () => patientService.getAll(params),
    select: (response) => response.data
  })
}

export const usePatient = (id: number) => {
  return useQuery({
    queryKey: ['patients', id],
    queryFn: () => patientService.getById(id),
    enabled: !!id,
    select: (response) => response.data
  })
}

export const useCreatePatient = () => {
  const queryClient = useQueryClient()
  
  return useMutation({
    mutationFn: patientService.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['patients'] })
      toast.success('Patient created successfully')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to create patient')
    }
  })
}
```

#### 4. Zustand Store

```typescript
// core/stores/authStore.ts
import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface AuthState {
  user: User | null
  token: string | null
  isAuthenticated: boolean
  selectedClinic: Clinic | null
  availableClinics: Clinic[]
}

interface AuthStore extends AuthState {
  login: (user: User, token: string, clinics?: Clinic[]) => void
  logout: () => void
  selectClinic: (clinic: Clinic) => void
}

export const useAuthStore = create<AuthStore>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      isAuthenticated: false,
      selectedClinic: null,
      availableClinics: [],

      login: (user, token, clinics = []) => {
        set({
          user,
          token,
          isAuthenticated: true,
          availableClinics: clinics,
          selectedClinic: clinics.length === 1 ? clinics[0] : null
        })
      },

      logout: () => {
        set({
          user: null,
          token: null,
          isAuthenticated: false,
          selectedClinic: null,
          availableClinics: []
        })
      },

      selectClinic: (clinic) => {
        set({ selectedClinic: clinic })
      }
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        user: state.user,
        token: state.token,
        isAuthenticated: state.isAuthenticated
      })
    }
  )
)
```

#### 5. Vite Configuration

```typescript
// vite.config.ts
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { VitePWA } from 'vite-plugin-pwa'
import path from 'path'

export default defineConfig({
  plugins: [
    react(),
    VitePWA({
      registerType: 'autoUpdate',
      manifest: {
        name: 'App Name',
        short_name: 'App',
        theme_color: '#1890ff'
      }
    })
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@core': path.resolve(__dirname, './src/core'),
      '@components': path.resolve(__dirname, './src/components'),
      '@services': path.resolve(__dirname, './src/core/services')
    }
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:7000',
        changeOrigin: true
      },
      '/queueHub': {
        target: 'ws://localhost:7000',
        ws: true
      }
    }
  }
})
```

#### 6. SignalR Integration

```typescript
// core/hooks/useSignalR.ts
import { useEffect, useRef } from 'react'
import * as signalR from '@microsoft/signalr'
import { useAuthStore } from '../stores/authStore'

export const useSignalR = (hubUrl: string) => {
  const connectionRef = useRef<signalR.HubConnection | null>(null)
  const authStore = useAuthStore()

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${hubUrl}?access_token=${authStore.token}`)
      .withAutomaticReconnect()
      .build()

    connection.start()
      .then(() => console.log('SignalR Connected'))
      .catch(err => console.error('SignalR Connection Error:', err))

    connectionRef.current = connection

    return () => {
      connection.stop()
    }
  }, [hubUrl, authStore.token])

  return connectionRef.current
}
```

---

## Database Architecture

### Multi-Tenant Database Strategy

**Global Database:**
- Organizations table
- Subscriptions table
- GlobalMedicines table (shared reference data)
- System users

**Tenant Databases:**
- One database per organization/tenant
- All tenant-specific data (patients, appointments, inventory, etc.)
- Isolated by physical database separation

**Connection String Pattern:**
```
GlobalConnection: "Server=...;Database=GlobalDB;..."
TenantConnection: "Server=...;Database=ClinicCare_{TenantId};..."
```

**Scaling:**
- Use Azure SQL Elastic Pools for cost efficiency
- 50-100 tenant databases per elastic pool
- Automatic tenant database creation on organization registration

---

## Security & Authentication

### JWT Token Structure
```json
{
  "sub": "userId",
  "email": "user@example.com",
  "role": "Doctor",
  "organizationId": 123,
  "exp": 1234567890
}
```

### Authorization Patterns
- Role-based: `[Authorize(Roles = "Doctor,Admin")]`
- Policy-based: `[Authorize(Policy = "RequireClinicAccess")]`
- Resource-based: Check ownership in handlers

### Tenant Isolation
1. **Middleware**: Resolves tenant from subdomain/header
2. **Database**: Separate databases per tenant
3. **Query Filters**: EF Core global query filters
4. **Validation**: Verify tenant context in handlers

---

## Background Jobs (Hangfire)

### Job Registration
```csharp
// In Program.cs
recurringJobManager.AddOrUpdate(
    "appointment-reminders",
    () => notificationJobs.SendAppointmentRemindersAsync(CancellationToken.None),
    Cron.Hourly);
```

### Job Implementation
```csharp
// Infrastructure/Jobs/NotificationJobs.cs
public class NotificationJobs
{
    private readonly ITenantDbContext _context;
    private readonly INotificationService _notificationService;

    public async Task SendAppointmentRemindersAsync(CancellationToken cancellationToken)
    {
        // Job logic here
    }
}
```

---

## Error Handling

### Backend Exception Middleware
```csharp
public class ExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

### Frontend Error Handling
- React Query error handling in hooks
- Axios interceptors for global error handling
- Toast notifications for user feedback
- Error boundaries for component-level errors

---

## Testing Strategy

### Backend
- Unit tests for handlers
- Integration tests for endpoints
- Repository pattern enables easy mocking

### Frontend
- Component tests with React Testing Library
- Hook tests for custom hooks
- E2E tests (optional, with Playwright/Cypress)

---

## Deployment Considerations

### Environment Configuration
- `appsettings.Development.json` - Local development
- `appsettings.Production.json` - Production
- `appsettings.Azure.json` - Azure-specific

### Frontend Configuration
- Runtime config via `public/config.js`
- Environment variables via Vite
- Build-time config generation script

### Database Migrations
- EF Core migrations for schema changes
- Separate migration scripts for global and tenant databases
- Automated migration on deployment

---

## Best Practices

### Backend
1. **Always use Result<T> pattern** for handler responses
2. **Validate inputs** with FluentValidation
3. **Use async/await** consistently
4. **Implement tenant isolation** at multiple layers
5. **Log important operations** with Serilog
6. **Use dependency injection** for all services
7. **Keep handlers focused** - single responsibility
8. **Use DTOs** for API responses, never expose entities directly

### Frontend
1. **Use React Query** for all server state
2. **Keep components small** and focused
3. **Use TypeScript** strictly - avoid `any`
4. **Extract reusable logic** into custom hooks
5. **Use Zustand** for client-side state only
6. **Handle loading/error states** in all queries
7. **Optimize re-renders** with React.memo when needed
8. **Use path aliases** for clean imports

### Database
1. **Index frequently queried columns**
2. **Use connection pooling**
3. **Enable retry logic** for transient failures
4. **Monitor query performance**
5. **Use transactions** for multi-step operations
6. **Implement soft deletes** with IsActive flag

---

## Project Initialization Checklist

### Backend Setup
- [ ] Create solution with 4 projects (API, Application, Domain, Infrastructure)
- [ ] Configure NuGet packages
- [ ] Set up dependency injection
- [ ] Configure database contexts
- [ ] Implement tenant middleware
- [ ] Set up JWT authentication
- [ ] Configure SignalR
- [ ] Set up Hangfire
- [ ] Create base entities
- [ ] Implement Result pattern
- [ ] Set up Serilog
- [ ] Configure CORS
- [ ] Create endpoint registry pattern

### Frontend Setup
- [ ] Initialize Vite + React + TypeScript project
- [ ] Install core dependencies
- [ ] Configure path aliases
- [ ] Set up API client with interceptors
- [ ] Create auth store (Zustand)
- [ ] Set up React Query provider
- [ ] Configure routing (TanStack Router)
- [ ] Set up Ant Design theme
- [ ] Create service layer structure
- [ ] Set up SignalR connection hook
- [ ] Configure PWA (optional)

### Database Setup
- [ ] Create global database
- [ ] Create first tenant database
- [ ] Run EF Core migrations
- [ ] Seed initial data
- [ ] Configure connection strings

---

## Example Feature Implementation

### Backend: Create Patient Feature

1. **Domain Entity** (`Domain/Entities/Patient.cs`)
2. **Command** (`Application/Features/Patients/Commands/CreatePatient/CreatePatientCommand.cs`)
3. **Validator** (`Application/Features/Patients/Commands/CreatePatient/CreatePatientCommandValidator.cs`)
4. **Handler** (`Application/Features/Patients/Commands/CreatePatient/CreatePatientHandler.cs`)
5. **DTO** (`Application/Features/Patients/Queries/GetPatient/PatientDto.cs`)
6. **Endpoint** (`API/Modules/Patients/PatientsEndpoints.cs`)

### Frontend: Patient List Feature

1. **Service** (`core/services/patientService.ts`)
2. **Query Hook** (`core/hooks/queries/usePatients.ts`)
3. **Component** (`apps/tenant/patients/PatientList.tsx`)
4. **Type Definitions** (`core/types/patient.ts`)

---

## Notes

- This architecture supports **horizontal scaling** through multi-tenant database separation
- **CQRS pattern** provides clear separation of read/write operations
- **Minimal APIs** reduce boilerplate compared to controllers
- **React Query** handles caching, refetching, and optimistic updates automatically
- **Zustand** provides lightweight state management without Redux complexity
- **SignalR** enables real-time features (notifications, live updates)
- **Hangfire** handles background processing without external dependencies

---

**Use this prompt as a template for any new project requiring similar architecture and technology stack.**

