# AI Model Instructions: ClinicCare Homoeopathy Clinic Management System

## 🎯 **PROJECT OVERVIEW**

**ClinicCare** is a comprehensive multi-tenant homoeopathy clinic management system designed to handle multiple organizations, each with multiple clinics, supporting doctors who work across different clinics. The system manages patient registration, appointments, medical records, prescriptions, inventory, billing, and communication.

## 🏗️ **ARCHITECTURE PATTERN**

### **Backend: Clean Architecture (.NET 9)**
- **Domain Layer**: Core business entities and rules
- **Application Layer**: Use cases, commands, queries (CQRS with MediatR)
- **Infrastructure Layer**: Data access, external services, implementations
- **API Layer**: Controllers, middleware, SignalR hubs

### **Frontend: Modern React Stack**
- **React 19** with TypeScript
- **Vite** for build tooling
- **TanStack Suite**: Query, Router, Table, Form
- **Zustand** for state management
- **Ant Design** for UI components

### **Database: Multi-Tenant SQL Server**
- Single database with tenant isolation via `OrganizationId`
- Row-level security through application logic
- Global vs. tenant-specific data separation

## 🔑 **CORE CONCEPTS**

### **Multi-Tenancy Strategy**
- **Subdomain-based resolution**: `organization1.yourapp.com`
- **Tenant isolation**: All data filtered by `OrganizationId`
- **Cross-organization access**: Doctors and patients can access multiple organizations
- **Clinic selection**: Users select clinic after login (if multiple clinics exist)

### **User Management**
- **Unified accounts**: One account per user across organizations
- **Role-based access**: Super Admin, Organization Admin, Doctor, Staff, Patient
- **Organization-scoped permissions**: Users see only their organization's data

### **Data Isolation**
- **TenantEntity base class**: All tenant-specific entities inherit from this
- **Automatic filtering**: DbContext automatically applies tenant filters
- **Global entities**: Medicines, system configurations shared across tenants

## 📊 **DOMAIN ENTITIES**

### **Core Entities**
```csharp
// Tenant-scoped entities (inherit from TenantEntity)
Organization, Clinic, User, Patient, Appointment, Consultation, 
Prescription, Inventory, Invoice, Communication

// Global entities (shared across tenants)
GlobalMedicine, SystemConfiguration
```

### **Key Relationships**
- **Organization** → **Clinics** (1:many)
- **User** → **UserOrganization** (many:many)
- **Doctor** → **DoctorProfile** → **DoctorAvailability** (1:1:many)
- **Patient** → **Appointments** → **Consultations** → **Prescriptions** (1:many:many:many)
- **Clinic** → **Inventory** → **ClinicMedicine** (1:many:many)

## 🚀 **TECHNICAL IMPLEMENTATION**

### **Backend Patterns**
- **CQRS**: Commands for writes, Queries for reads
- **MediatR**: Mediator pattern for command/query handling
- **Repository Pattern**: Generic repository with tenant filtering
- **Unit of Work**: DbContext manages transactions
- **Specification Pattern**: Complex query building

### **Authentication & Authorization**
- **JWT Tokens**: Stateless authentication
- **Role-based permissions**: Claims-based authorization
- **Tenant context**: Automatically resolved from subdomain
- **Current user service**: Injected into all handlers

### **Real-time Features**
- **SignalR**: Real-time queue updates, notifications
- **Hub methods**: Queue updates, appointment status changes
- **Client groups**: Per-clinic, per-doctor grouping

## 🔧 **DEVELOPMENT WORKFLOW**

### **Adding New Features**
1. **Domain**: Define entities, enums, interfaces
2. **Application**: Create commands/queries, handlers, validators
3. **Infrastructure**: Implement repositories, services
4. **API**: Add controllers, update middleware if needed
5. **Frontend**: Create components, services, stores

### **Database Changes**
1. **Entity modifications**: Update domain entities
2. **Migrations**: Generate EF Core migrations
3. **Seed data**: Update database seeding scripts
4. **Testing**: Verify tenant isolation still works

### **API Development**
1. **Controller**: Define endpoints with proper authorization
2. **Validation**: Use FluentValidation for input validation
3. **Error handling**: Consistent error responses via ExceptionMiddleware
4. **Documentation**: Swagger annotations for API documentation

## ⚠️ **CRITICAL CONSTRAINTS**

### **Multi-Tenancy Rules**
- **NEVER** expose data from other tenants
- **ALWAYS** filter by `OrganizationId` in queries
- **ALWAYS** set `OrganizationId` when creating new entities
- **NEVER** trust client-provided tenant information

### **Security Requirements**
- **Input validation**: All inputs must be validated
- **Authorization checks**: Verify user permissions for each operation
- **SQL injection prevention**: Use parameterized queries only
- **Audit trails**: Log all data modifications

### **Performance Considerations**
- **Database indexes**: Tenant-specific indexes on `OrganizationId`
- **Query optimization**: Avoid N+1 queries in tenant-scoped operations
- **Caching**: Tenant-scoped caching strategies
- **Connection pooling**: Efficient database connection management

## 🧪 **TESTING STRATEGY**

### **Unit Testing**
- **Domain logic**: Test business rules and validations
- **Application handlers**: Test command/query processing
- **Infrastructure services**: Test tenant isolation and data access

### **Integration Testing**
- **API endpoints**: Test with different tenant contexts
- **Database operations**: Verify tenant filtering works correctly
- **Authentication flow**: Test JWT token handling

### **Multi-Tenant Testing**
- **Tenant isolation**: Verify data separation between organizations
- **Cross-tenant access**: Test doctor/patient access across organizations
- **Clinic selection**: Verify clinic-specific data filtering

## 🚨 **COMMON PITFALLS**

### **Tenant Isolation Issues**
- **Missing OrganizationId filter**: Always include in WHERE clauses
- **Global queries**: Remember to scope to current tenant
- **Cached data**: Ensure tenant context is maintained in caching

### **Authentication Problems**
- **Missing authorization attributes**: All endpoints must have proper authorization
- **Token validation**: Verify JWT token structure and claims
- **Tenant resolution**: Ensure subdomain is properly resolved

### **Performance Issues**
- **Missing indexes**: Add indexes on `OrganizationId` columns
- **Inefficient queries**: Use Include() and projection for related data
- **Memory leaks**: Dispose of DbContext properly

## 📚 **KEY FILES TO UNDERSTAND**

### **Backend Core**
- `src/ClinicCare.Domain/Entities/` - All domain entities
- `src/ClinicCare.Application/Features/` - Use cases and handlers
- `src/ClinicCare.Infrastructure/Data/ApplicationDbContext.cs` - Database context
- `src/ClinicCare.API/Program.cs` - Application configuration and middleware

### **Frontend Core**
- `frontend/src/App.tsx` - Main application component
- `frontend/src/stores/` - Zustand state management
- `frontend/src/services/` - API service layer
- `frontend/src/components/` - Reusable UI components

### **Configuration**
- `src/ClinicCare.API/appsettings.json` - Database connection, JWT settings
- `frontend/package.json` - Frontend dependencies and scripts
- `database/` - SQL scripts for database setup

## 🎯 **DEVELOPMENT PRIORITIES**

### **Phase 1: Core Backend (Current)**
- ✅ Multi-tenant architecture
- ✅ Authentication system
- ✅ Basic CRUD operations
- 🚧 Complete API endpoints
- 🚧 Advanced business logic

### **Phase 2: Frontend Development**
- 🚧 User authentication flows
- 🚧 Role-based dashboards
- 🚧 Real-time queue management
- 🚧 Responsive design implementation

### **Phase 3: Advanced Features**
- 🚧 WhatsApp integration
- 🚧 Payment gateway integration
- 🚧 Reporting and analytics
- 🚧 Mobile app development

## 🔍 **DEBUGGING GUIDELINES**

### **Tenant Issues**
- Check `TenantMiddleware` execution in console logs
- Verify `OrganizationId` is being set correctly
- Check database queries include tenant filtering

### **Authentication Issues**
- Verify JWT token structure
- Check user claims and roles
- Ensure proper authorization attributes

### **Database Issues**
- Check connection string configuration
- Verify SQL Server version compatibility
- Check entity framework migrations

## 📖 **RESOURCES & REFERENCES**

### **Documentation**
- **.NET 9**: https://docs.microsoft.com/en-us/dotnet/
- **Entity Framework Core**: https://docs.microsoft.com/en-us/ef/core/
- **React 19**: https://react.dev/
- **TanStack**: https://tanstack.com/

### **Architecture Patterns**
- **Clean Architecture**: https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html
- **CQRS**: https://martinfowler.com/bliki/CQRS.html
- **Multi-Tenancy**: https://martinfowler.com/articles/multi-tenancy.html

---

## 🎯 **AI MODEL RESPONSIBILITIES**

When working with this project, the AI model should:

1. **Understand multi-tenancy**: Always consider tenant isolation in all operations
2. **Follow Clean Architecture**: Respect layer boundaries and dependencies
3. **Maintain security**: Ensure proper authorization and input validation
4. **Consider performance**: Optimize queries and avoid common pitfalls
5. **Follow patterns**: Use established patterns for consistency
6. **Test thoroughly**: Verify tenant isolation and security measures
7. **Document changes**: Update relevant documentation and comments

**Remember**: This is a production healthcare system - security, data integrity, and tenant isolation are paramount!



