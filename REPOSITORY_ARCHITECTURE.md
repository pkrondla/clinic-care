# Repository Architecture in ClinicCare

## 🏗️ **Repository Pattern Implementation**

### **1. Repository Interfaces (Application Layer)**
```
src/ClinicCare.Application/Common/Interfaces/
├── Global/
│   ├── IGlobalDbContext.cs           ← Global DB context interface
│   ├── IOrganizationRepository.cs    ← Organization repository
│   └── IGlobalMedicineRepository.cs  ← Global medicine repository
└── Tenant/
    ├── ITenantDbContext.cs           ← Tenant DB context interface
    ├── IAppointmentRepository.cs     ← Appointment repository
    └── IPatientRepository.cs         ← Patient repository
```

### **2. Repository Implementations (Infrastructure Layer)**
```
src/ClinicCare.Infrastructure/Data/
├── GlobalRepositories/
│   ├── OrganizationRepository.cs     ← Organization repository impl
│   └── GlobalMedicineRepository.cs   ← Global medicine repository impl
└── TenantRepositories/
    ├── AppointmentRepository.cs      ← Appointment repository impl
    └── PatientRepository.cs          ← Patient repository impl
```

### **3. Database Contexts**
```
src/ClinicCare.Infrastructure/Data/
├── GlobalDbContext.cs                ← Global database context
└── TenantDbContext.cs                ← Tenant database context
```

## 🎯 **Repository Pattern Types**

### **1. Generic Repository (IApplicationDbContext)**
- **Purpose**: Basic CRUD operations for all entities
- **Usage**: Simple operations, Unit of Work pattern
- **Example**: `_context.Appointments.Add(appointment)`

### **2. Specific Repository (IAppointmentRepository)**
- **Purpose**: Complex business queries and operations
- **Usage**: Business-specific logic, complex filtering
- **Example**: `_appointmentRepository.GetDoctorQueueAsync(doctorId, clinicId, date)`

## 📋 **Repository Methods Available**

### **IAppointmentRepository Interface**
```csharp
// Basic CRUD
Task<Appointment?> GetByIdAsync(int id);
Task<Appointment?> GetByIdWithDetailsAsync(int id);
Task<List<Appointment>> GetAllAsync();
Task<Appointment> AddAsync(Appointment appointment);
Task UpdateAsync(Appointment appointment);
Task DeleteAsync(int id);

// Business Queries
Task<List<Appointment>> GetByDoctorAndDateAsync(int doctorId, DateOnly date);
Task<List<Appointment>> GetByPatientAndDateAsync(int patientId, DateOnly date);
Task<List<Appointment>> GetByClinicAndDateAsync(int clinicId, DateOnly date);
Task<List<Appointment>> GetUpcomingAppointmentsAsync(int patientId);
Task<List<Appointment>> GetDoctorQueueAsync(int doctorId, int clinicId, DateOnly date);

// Business Validation
Task<bool> IsSlotAvailableAsync(int doctorId, int clinicId, DateOnly date, int tokenNumber);
Task<bool> HasConflictingAppointmentAsync(int doctorId, int clinicId, DateOnly date, int tokenNumber, int? excludeId = null);

// Statistics
Task<int> GetAppointmentCountByStatusAsync(int? clinicId, int? doctorId, int status);
Task<int> GetAppointmentCountByTypeAsync(int? clinicId, int? doctorId, int type);
Task<int> GetAppointmentCountByDateRangeAsync(int? clinicId, int? doctorId, DateOnly startDate, DateOnly endDate);
```

## 🔄 **How to Use Repositories**

### **Global Repository Example**
```csharp
public class CreateOrganizationHandler 
    : IRequestHandler<CreateOrganizationCommand, Result<OrganizationDto>>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IGlobalDbContext _globalDbContext;
    
    public async Task<Result<OrganizationDto>> Handle(
        CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        // Use global repository for business operations
        var exists = await _organizationRepository.ExistsBySubdomainAsync(
            request.Subdomain);
            
        if (exists)
            return Result<OrganizationDto>.Failure(new[] { "Subdomain taken" });
            
        // Use global context for data operations
        var organization = Organization.Create(request.Name, request.Subdomain);
        await _organizationRepository.AddAsync(organization);
        
        return Result<OrganizationDto>.Success(dto);
    }
}
```

### **Tenant Repository Example**
```csharp
public class CreateAppointmentHandler 
    : IRequestHandler<CreateAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ITenantService _tenantService;
    
    public async Task<Result<AppointmentDto>> Handle(
        CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        // Tenant context is already set by middleware
        var tenantId = _tenantService.GetTenantId();
        
        // Use tenant repository for business operations
        var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
            request.DoctorId, request.ClinicId, request.AppointmentDate, request.TokenNumber);
            
        if (hasConflict)
            return Result<AppointmentDto>.Failure(new[] { "Slot not available" });
            
        // Use tenant repository for data operations
        var appointment = Appointment.Create(tenantId, request.DoctorId, ...);
        await _appointmentRepository.AddAsync(appointment);
        
        return Result<AppointmentDto>.Success(dto);
    }
}
```

## 🚀 **Benefits of This Approach**

### **1. Separation of Concerns**
- ✅ **Application Layer**: Defines what operations are needed
- ✅ **Infrastructure Layer**: Implements how operations are performed
- ✅ **Domain Layer**: Contains business logic

### **2. Testability**
- ✅ **Mockable**: Easy to mock repository interfaces for unit tests
- ✅ **Isolated**: Business logic can be tested without database
- ✅ **Focused**: Each repository can be tested independently

### **3. Maintainability**
- ✅ **Single Responsibility**: Each repository handles one aggregate
- ✅ **Business-Focused**: Methods reflect business operations
- ✅ **Reusable**: Common operations can be reused across handlers

### **4. Performance**
- ✅ **Optimized Queries**: Repository methods can be optimized for specific use cases
- ✅ **Caching**: Repository can implement caching strategies
- ✅ **Lazy Loading**: Can control when related data is loaded

## � **Database Strategy**

### **1. Global Database**
- **Purpose**: Manages system-wide data and tenant information
- **Name**: `ClinicCare_Global`
- **Key Tables**:
  ```sql
  Organizations         -- Healthcare organizations using the system
  Subscriptions        -- Subscription plans and billing
  GlobalMedicines      -- Common medicine database
  SystemUsers          -- Super admin and global system users
  AuditLogs           -- System-wide audit trail
  ```

### **2. Tenant Databases**
- **Purpose**: Isolated clinic data per organization
- **Naming**: `ClinicCare_{tenantId}`
- **Key Tables**:
  ```sql
  Clinics             -- Clinics in the organization
  Doctors             -- Doctors working in clinics
  Patients           -- Patient records
  Appointments       -- Appointments and tokens
  Prescriptions      -- Patient prescriptions
  Inventory          -- Per-clinic medicine inventory
  ```

## 📁 **Project Structure**

```
ClinicCare.sln
├── src/
│   ├── ClinicCare.Domain/                    ← Domain Layer
│   │   ├── Common/
│   │   │   └── BaseEntity.cs
│   │   ├── Global/                           ← Global Domain Entities
│   │   │   ├── Organization.cs
│   │   │   ├── Subscription.cs
│   │   │   └── GlobalMedicine.cs
│   │   └── Tenant/                           ← Tenant Domain Entities
│   │       ├── Appointment.cs
│   │       ├── Patient.cs
│   │       └── Prescription.cs
│   │
│   ├── ClinicCare.Application/               ← Application Layer
│   │   ├── Common/
│   │   │   └── Interfaces/
│   │   │       ├── IGlobalDbContext.cs       ← Global DB context
│   │   │       └── ITenantDbContext.cs       ← Tenant DB context
│   │   ├── Global/                           ← Global Features
│   │   │   ├── Organizations/
│   │   │   └── Subscriptions/
│   │   └── Tenant/                           ← Tenant Features
│   │       ├── Appointments/
│   │       └── Patients/
│   │
│   ├── ClinicCare.Infrastructure/            ← Infrastructure Layer
│   │   ├── Data/
│   │   │   ├── GlobalDbContext.cs           ← Global DB implementation
│   │   │   ├── TenantDbContext.cs           ← Tenant DB implementation
│   │   │   ├── GlobalRepositories/          ← Global repositories
│   │   │   └── TenantRepositories/          ← Tenant repositories
│   │   └── Services/
│   │       ├── TenantService.cs             ← Tenant resolution
│   │       └── DatabaseManager.cs            ← DB management
│   │
│   └── ClinicCare.API/                       ← API Layer
│       ├── Modules/                          ← Feature modules
│       │   ├── Global/                       ← Global endpoints
│       │   │   ├── Organizations/
│       │   │   └── Subscriptions/
│       │   └── Tenant/                       ← Tenant endpoints
│       │       ├── Appointments/
│       │       └── Patients/
│       └── Middleware/
│           └── TenantMiddleware.cs           ← Tenant resolution
│
└── frontend/                                 ← Frontend
    ├── src/
    │   ├── apps/
    │   │   ├── global/                      ← Global admin app
    │   │   └── tenant/                      ← Tenant clinic app
    │   ├── components/                      ← Shared components
    │   └── services/
    │       ├── globalApi.ts                 ← Global API client
    │       └── tenantApi.ts                 ← Tenant API client
```

## 🎯 **Next Steps**

1. **Create More Repositories**: Add repositories for other entities (Patient, Doctor, etc.)
2. **Add Caching**: Implement caching in repository methods
3. **Add Unit Tests**: Create unit tests for repository methods
4. **Add Logging**: Add logging to repository operations
5. **Add Metrics**: Add performance metrics to repository methods

The repository pattern is now properly implemented and ready to use! 🚀

