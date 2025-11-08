# Repository Architecture in ClinicCare

## 🏗️ **Repository Pattern Implementation**

### **1. Repository Interfaces (Application Layer)**
```
src/ClinicCare.Application/Common/Interfaces/
├── IApplicationDbContext.cs          ← Generic repository interface
└── IAppointmentRepository.cs         ← Specific repository interface
```

### **2. Repository Implementations (Infrastructure Layer)**
```
src/ClinicCare.Infrastructure/Data/Repositories/
└── AppointmentRepository.cs          ← Specific repository implementation
```

### **3. Generic Repository (DbContext)**
```
src/ClinicCare.Infrastructure/Data/
└── ApplicationDbContext.cs           ← Generic repository implementation
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

### **In Command Handlers**
```csharp
public class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    
    public async Task<Result<AppointmentDto>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        // Use repository for business operations
        var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
            request.DoctorId, request.ClinicId, request.AppointmentDate, request.TokenNumber);
            
        if (hasConflict)
            return Result<AppointmentDto>.Failure(new[] { "Slot not available" });
            
        // Use repository for data operations
        var appointment = Appointment.Create(...);
        await _appointmentRepository.AddAsync(appointment);
        
        return Result<AppointmentDto>.Success(dto);
    }
}
```

### **In Query Handlers**
```csharp
public class GetAppointmentsHandler : IRequestHandler<GetAppointmentsQuery, Result<List<AppointmentDto>>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    
    public async Task<Result<List<AppointmentDto>>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        // Use repository for complex queries
        var appointments = await _appointmentRepository.GetByDoctorAndDateAsync(
            request.DoctorId, request.Date);
            
        return Result<List<AppointmentDto>>.Success(appointments.Select(MapToDto).ToList());
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

## 📁 **File Structure**

```
ClinicCare.sln
├── ClinicCare.Application/
│   └── Common/
│       └── Interfaces/
│           ├── IApplicationDbContext.cs      ← Generic repository
│           └── IAppointmentRepository.cs     ← Specific repository
├── ClinicCare.Infrastructure/
│   └── Data/
│       ├── ApplicationDbContext.cs           ← Generic repository implementation
│       └── Repositories/
│           └── AppointmentRepository.cs      ← Specific repository implementation
└── ClinicCare.API/
    └── Controllers/
        └── Appointments/
            └── AppointmentsController.cs     ← Uses repositories through handlers
```

## 🎯 **Next Steps**

1. **Create More Repositories**: Add repositories for other entities (Patient, Doctor, etc.)
2. **Add Caching**: Implement caching in repository methods
3. **Add Unit Tests**: Create unit tests for repository methods
4. **Add Logging**: Add logging to repository operations
5. **Add Metrics**: Add performance metrics to repository methods

The repository pattern is now properly implemented and ready to use! 🚀

