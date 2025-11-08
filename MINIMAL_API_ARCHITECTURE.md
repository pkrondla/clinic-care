# Minimal API Architecture for ClinicCare

## 🚀 **Overview**

The ClinicCare API has been refactored to use **Minimal APIs** with a **Modular Monolith + Vertical Slices + DDD** architecture. This provides better performance, cleaner code, and easier maintenance.

## 🏗️ **Architecture Structure**

```
ClinicCare.API/
├── Modules/                           ← Module-based organization
│   ├── Appointments/
│   │   ├── AppointmentsEndpoints.cs           ← Basic CRUD endpoints
│   │   ├── AppointmentsEndpointsAdvanced.cs   ← Advanced features
│   │   └── AppointmentsEndpointsExtensions.cs ← Extensions & validation
│   ├── Patients/
│   │   └── PatientsEndpoints.cs
│   ├── Inventory/
│   │   └── InventoryEndpoints.cs
│   └── Billing/
│       └── BillingEndpoints.cs
├── Endpoints/
│   └── EndpointsRegistry.cs          ← Central registry
└── Program.cs                         ← Main configuration
```

## 🎯 **Key Features**

### **1. Module-Based Organization**
- ✅ **Vertical Slices**: Each module contains all related endpoints
- ✅ **Separation of Concerns**: Clear boundaries between modules
- ✅ **Team Ownership**: Teams can own entire modules

### **2. Advanced Minimal API Features**
- ✅ **OpenAPI Integration**: Automatic Swagger documentation
- ✅ **Rate Limiting**: Built-in rate limiting per endpoint
- ✅ **Caching**: Response caching for performance
- ✅ **Validation**: Built-in request validation
- ✅ **Authorization**: JWT-based authentication

### **3. Performance Optimizations**
- ✅ **Memory Caching**: Cached responses for frequently accessed data
- ✅ **Rate Limiting**: Protection against abuse
- ✅ **Async/Await**: Non-blocking operations
- ✅ **Efficient Serialization**: Optimized JSON serialization

## 📋 **Available Endpoints**

### **Appointments Module**

#### **Basic CRUD Operations**
- `POST /api/appointments` - Create appointment
- `GET /api/appointments` - Get appointments with filtering
- `GET /api/appointments/{id}` - Get specific appointment
- `PUT /api/appointments/{id}` - Update appointment
- `DELETE /api/appointments/{id}` - Cancel appointment
- `GET /api/appointments/stats` - Get appointment statistics

#### **Advanced Operations**
- `GET /api/appointments/search` - Advanced search with pagination
- `GET /api/appointments/analytics` - Detailed analytics
- `GET /api/appointments/queue/{doctorId}/{clinicId}` - Doctor queue
- `GET /api/appointments/export` - Export appointments

### **Other Modules**
- **Patients**: `/api/patients/*` (Coming soon)
- **Inventory**: `/api/inventory/*` (Coming soon)
- **Billing**: `/api/billing/*` (Coming soon)

## 🔧 **Configuration**

### **Rate Limiting**
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AppointmentsPolicy", policy =>
    {
        policy.PermitLimit = 100;
        policy.Window = TimeSpan.FromMinutes(1);
        policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        policy.QueueLimit = 10;
    });
});
```

### **Caching**
```csharp
// Memory cache for frequently accessed data
builder.Services.AddMemoryCache();

// Response caching
.CacheOutput(policy => policy
    .Expire(TimeSpan.FromMinutes(5))
    .SetVaryByQuery("*"))
```

### **Validation**
```csharp
// Built-in validation attributes
public class CreateAppointmentRequest
{
    [Required]
    public int ClinicId { get; set; }
    
    [Required]
    [Range(1, 999)]
    public int TokenNumber { get; set; }
    
    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;
}
```

## 🚀 **Benefits of Minimal APIs**

### **1. Performance**
- ✅ **Faster Startup**: No controller overhead
- ✅ **Lower Memory Usage**: Reduced object allocation
- ✅ **Better Throughput**: Optimized request handling

### **2. Developer Experience**
- ✅ **Less Boilerplate**: Cleaner, more concise code
- ✅ **Better IntelliSense**: Improved IDE support
- ✅ **Easier Testing**: Simpler unit testing

### **3. Maintainability**
- ✅ **Modular Design**: Easy to add/remove features
- ✅ **Clear Structure**: Obvious code organization
- ✅ **Team Collaboration**: Clear module boundaries

## 📊 **Example Usage**

### **Create Appointment**
```http
POST /api/appointments
Content-Type: application/json
Authorization: Bearer <token>

{
  "clinicId": 1,
  "doctorId": 2,
  "patientId": 3,
  "appointmentDate": "2024-01-15",
  "tokenNumber": 5,
  "type": 1,
  "notes": "Regular checkup"
}
```

### **Search Appointments**
```http
GET /api/appointments/search?clinicId=1&doctorId=2&startDate=2024-01-01&endDate=2024-01-31&page=1&pageSize=20&sortBy=date&sortOrder=asc
Authorization: Bearer <token>
```

### **Get Doctor Queue**
```http
GET /api/appointments/queue/2/1?date=2024-01-15
Authorization: Bearer <token>
```

## 🔄 **Migration from Controllers**

### **Before (Controller)**
```csharp
[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
```

### **After (Minimal API)**
```csharp
public static class AppointmentsEndpoints
{
    public static IEndpointRouteBuilder MapAppointmentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments")
            .WithTags("Appointments")
            .RequireAuthorization();

        group.MapPost("/", CreateAppointment)
            .WithName("CreateAppointment")
            .WithSummary("Create a new appointment");

        return app;
    }

    private static async Task<IResult> CreateAppointment(
        CreateAppointmentCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(result);
    }
}
```

## 🎯 **Next Steps**

1. **Add More Modules**: Implement Patients, Inventory, Billing modules
2. **Add Validation**: Implement FluentValidation for complex validation
3. **Add Logging**: Add structured logging to endpoints
4. **Add Metrics**: Add performance metrics and monitoring
5. **Add Tests**: Create integration tests for endpoints
6. **Add Documentation**: Generate comprehensive API documentation

## 🏆 **Best Practices**

### **1. Endpoint Organization**
- ✅ **Group Related Endpoints**: Use MapGroup for logical grouping
- ✅ **Consistent Naming**: Use clear, descriptive names
- ✅ **Proper HTTP Verbs**: Use appropriate HTTP methods

### **2. Error Handling**
- ✅ **Consistent Responses**: Use Results.Ok, Results.BadRequest, etc.
- ✅ **Proper Status Codes**: Return appropriate HTTP status codes
- ✅ **Error Messages**: Provide clear error messages

### **3. Performance**
- ✅ **Use Caching**: Cache frequently accessed data
- ✅ **Rate Limiting**: Protect against abuse
- ✅ **Async Operations**: Use async/await for I/O operations

The minimal API architecture is now ready and provides a modern, performant, and maintainable API! 🚀

