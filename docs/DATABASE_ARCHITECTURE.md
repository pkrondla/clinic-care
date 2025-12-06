# ClinicCare Database Architecture

## 🏗️ Azure SQL Database Architecture

### **Production Architecture**

```
┌─────────────────────────────────────────────────────────────────┐
│                     Azure SQL Server                             │
│              cliniccare-prod.database.windows.net                │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
        ▼                     ▼                     ▼
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│   Global DB  │    │ Elastic Pool │    │ Elastic Pool │
│   (S2 - 50   │    │   #1 (100    │    │   #2 (100    │
│     DTU)     │    │     DTU)     │    │     DTU)     │
└──────────────┘    └──────────────┘    └──────────────┘
      │                     │                     │
      │                     │                     │
      ▼                     ▼                     ▼
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│Organizations │    │Tenant DBs    │    │Tenant DBs    │
│Subscriptions │    │1-50          │    │51-100        │
│GlobalMedicines│   │              │    │              │
└──────────────┘    │• Org1        │    │• Org51       │
                    │• Org2        │    │• Org52       │
                    │• ...         │    │• ...         │
                    │• Org50       │    │• Org100      │
                    └──────────────┘    └──────────────┘
```

---

## 📊 Tenant Database Structure

### **Each Tenant Database Contains:**

```
ClinicCare_Org1
├── Clinics
│   ├── ClinicId (PK)
│   ├── Name
│   ├── Address
│   └── ContactInfo
│
├── Users
│   ├── UserId (PK)
│   ├── Email
│   ├── Role
│   └── OrganizationId
│
├── Patients
│   ├── PatientId (PK)
│   ├── PatientCode
│   ├── Demographics
│   └── MedicalHistory
│
├── Appointments
│   ├── AppointmentId (PK)
│   ├── PatientId (FK)
│   ├── DoctorId (FK)
│   ├── ClinicId (FK)
│   └── DateTime
│
├── Consultations
│   ├── ConsultationId (PK)
│   ├── AppointmentId (FK)
│   ├── Diagnosis
│   └── Treatment
│
├── Prescriptions
│   ├── PrescriptionId (PK)
│   ├── ConsultationId (FK)
│   └── Medicines
│
├── Inventory
│   ├── InventoryId (PK)
│   ├── ClinicId (FK)
│   ├── MedicineId
│   └── Quantity
│
└── Invoices
    ├── InvoiceId (PK)
    ├── PatientId (FK)
    ├── Amount
    └── Status
```

---

## 🔄 Data Flow

### **1. User Login Flow**

```
User Request (subdomain: org1.cliniccare.com)
        │
        ▼
┌─────────────────┐
│ Tenant Resolver │
│ (Middleware)    │
└─────────────────┘
        │
        ▼ Extract subdomain: "org1"
┌─────────────────┐
│  Global DB      │ ──► Query Organizations table
│  Connection     │     WHERE Subdomain = 'org1'
└─────────────────┘
        │
        ▼ Get OrganizationId: 123
┌─────────────────┐
│  Tenant DB      │ ──► Connect to: ClinicCare_org1
│  Connection     │     Query Users WHERE OrganizationId = 123
└─────────────────┘
        │
        ▼
   Return User Data
```

---

### **2. Patient Registration Flow**

```
POST /api/patients
        │
        ▼
┌─────────────────┐
│  API Endpoint   │
└─────────────────┘
        │
        ▼
┌─────────────────┐
│  MediatR        │ ──► CreatePatientCommand
│  (CQRS)         │
└─────────────────┘
        │
        ▼
┌─────────────────┐
│  Handler        │ ──► Business Logic
│                 │     • Validate data
│                 │     • Generate PatientCode
│                 │     • Hash password
└─────────────────┘
        │
        ▼
┌─────────────────┐
│  Tenant DB      │ ──► INSERT INTO Patients
│  (ClinicCare_   │     INSERT INTO Users
│   org1)         │
└─────────────────┘
        │
        ▼
   Return PatientDto
```

---

### **3. Cross-Clinic Access Flow**

```
Doctor logs in at Clinic A
        │
        ▼
┌─────────────────┐
│  Get Doctor     │ ──► Query: UserClinicAccess
│  Clinic Access  │     WHERE UserId = doctorId
└─────────────────┘
        │
        ▼
    Returns: [Clinic A, Clinic B, Clinic C]
        │
        ▼
┌─────────────────┐
│  Doctor selects │
│  Clinic B       │
└─────────────────┘
        │
        ▼
┌─────────────────┐
│  Query Patients │ ──► SELECT * FROM Patients
│  for Clinic B   │     JOIN Appointments
│                 │     WHERE ClinicId = clinicB.Id
└─────────────────┘
        │
        ▼
   Display Patients from Clinic B
```

---

## 🔐 Data Isolation

### **Multi-Level Isolation**

```
Level 1: Physical Database Separation
┌────────────────────────────────────────┐
│  ClinicCare_Org1  │  ClinicCare_Org2  │
│  (Separate DB)    │  (Separate DB)    │
└────────────────────────────────────────┘
        ✅ Cannot query each other's data

Level 2: OrganizationId Filter
┌────────────────────────────────────────┐
│  All entities have OrganizationId      │
│  Global query filter applied           │
└────────────────────────────────────────┘
        ✅ Double protection

Level 3: Tenant Middleware
┌────────────────────────────────────────┐
│  Subdomain → OrganizationId            │
│  Validated on every request            │
└────────────────────────────────────────┘
        ✅ Request-level validation
```

---

## 📈 Scaling Strategy

### **Growth Path**

```
Stage 1: 1-50 Tenants
┌──────────────────────┐
│  1 Elastic Pool      │
│  (100 DTU)           │
│  Cost: $465/month    │
└──────────────────────┘

Stage 2: 51-100 Tenants
┌──────────────────────┬──────────────────────┐
│  Elastic Pool #1     │  Elastic Pool #2     │
│  (100 DTU)           │  (100 DTU)           │
│  50 DBs              │  50 DBs              │
└──────────────────────┴──────────────────────┘
Cost: $930/month

Stage 3: 101-200 Tenants
┌──────────────────────┬──────────────────────┐
│  Pool #1 (200 DTU)   │  Pool #2 (200 DTU)   │
│  100 DBs             │  100 DBs             │
└──────────────────────┴──────────────────────┘
Cost: $1,860/month

Stage 4: 200+ Tenants (Geographic Distribution)
┌─────────────────────────────────────────────┐
│  US East                                    │
│  ├── Pool #1 (100 DBs)                     │
│  └── Pool #2 (100 DBs)                     │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│  US West                                    │
│  ├── Pool #3 (100 DBs)                     │
│  └── Pool #4 (100 DBs)                     │
└─────────────────────────────────────────────┘
```

---

## 💰 Cost Optimization

### **Elastic Pool Resource Sharing**

```
Traditional Approach:
┌────────┐ ┌────────┐ ┌────────┐
│ DB1    │ │ DB2    │ │ DB3    │
│ 50 DTU │ │ 50 DTU │ │ 50 DTU │
│ $75/mo │ │ $75/mo │ │ $75/mo │
└────────┘ └────────┘ └────────┘
Total: $225/month for 3 DBs

Elastic Pool Approach:
┌──────────────────────────────┐
│  Elastic Pool (100 DTU)      │
│  ├── DB1 (uses 20 DTU)       │
│  ├── DB2 (uses 30 DTU)       │
│  └── DB3 (uses 10 DTU)       │
│  Total: 60 DTU used          │
│  Cost: $465/month            │
└──────────────────────────────┘
Can fit 10-20 small DBs in same pool
Cost per DB: $23-46/month

Savings: 40-70% compared to individual DBs
```

---

## 🔄 Backup & Recovery

### **Automated Backup Strategy**

```
┌─────────────────────────────────────────────┐
│  Automatic Backups (Azure SQL)              │
├─────────────────────────────────────────────┤
│  • Full backup: Weekly                      │
│  • Differential backup: Every 12 hours      │
│  • Transaction log: Every 5-10 minutes      │
│  • Retention: 7-35 days (configurable)      │
│  • Point-in-time restore: Any second        │
└─────────────────────────────────────────────┘
        │
        ▼
┌─────────────────────────────────────────────┐
│  Long-Term Retention (Optional)             │
├─────────────────────────────────────────────┤
│  • Weekly: 12 weeks                         │
│  • Monthly: 12 months                       │
│  • Yearly: 10 years                         │
│  • Compliance: HIPAA, GDPR                  │
└─────────────────────────────────────────────┘
```

### **Recovery Scenarios**

```
Scenario 1: Accidental Data Deletion
┌────────────────────────────────────┐
│  Point-in-Time Restore             │
│  Restore to 5 minutes before       │
│  deletion                          │
│  Time: 5-10 minutes                │
└────────────────────────────────────┘

Scenario 2: Database Corruption
┌────────────────────────────────────┐
│  Restore from last good backup     │
│  Automatic corruption detection    │
│  Time: 10-30 minutes               │
└────────────────────────────────────┘

Scenario 3: Tenant Wants Data Export
┌────────────────────────────────────┐
│  Database Copy                     │
│  Create read-only copy             │
│  Export to BACPAC                  │
│  Time: 15-60 minutes               │
└────────────────────────────────────┘

Scenario 4: Regional Disaster
┌────────────────────────────────────┐
│  Geo-Restore                       │
│  Restore to different region       │
│  From geo-replicated backup        │
│  Time: 1-2 hours                   │
└────────────────────────────────────┘
```

---

## 🔍 Monitoring

### **Key Metrics to Monitor**

```
Database Level:
├── DTU Consumption (target: <80%)
├── Storage Usage (alert at 80%)
├── Connection Count (alert at 90% of limit)
├── Deadlocks (alert if >5/hour)
└── Query Performance (alert if >2s avg)

Elastic Pool Level:
├── Pool DTU Usage (target: <80%)
├── Per-Database DTU Distribution
├── Storage Allocation
└── Database Count

Application Level:
├── Response Time (target: <500ms)
├── Error Rate (target: <1%)
├── Active Users
└── Request Rate
```

---

## ✅ Best Practices

### **1. Connection Management**
```csharp
// ✅ Good: Use connection pooling
services.AddDbContext<TenantDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(5);
        sqlOptions.CommandTimeout(30);
    }));

// ❌ Bad: Creating new connections each time
using var connection = new SqlConnection(connectionString);
```

### **2. Query Optimization**
```csharp
// ✅ Good: Use AsNoTracking for read-only queries
var patients = await context.Patients
    .AsNoTracking()
    .Where(p => p.ClinicId == clinicId)
    .ToListAsync();

// ❌ Bad: Loading unnecessary data
var patients = await context.Patients
    .Include(p => p.Appointments)
        .ThenInclude(a => a.Consultations)
    .ToListAsync(); // Loads everything!
```

### **3. Tenant Isolation**
```csharp
// ✅ Good: Automatic tenant filtering
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.Entity<Patient>()
        .HasQueryFilter(p => p.OrganizationId == _tenantService.OrganizationId);
}

// ❌ Bad: Manual filtering everywhere
var patients = await context.Patients
    .Where(p => p.OrganizationId == organizationId) // Easy to forget!
    .ToListAsync();
```

---

## 🚀 Deployment Checklist

- [ ] Azure SQL Server provisioned
- [ ] Firewall rules configured
- [ ] Global database created and migrated
- [ ] Elastic Pool created
- [ ] First tenant database created
- [ ] Connection strings configured in Key Vault
- [ ] Managed Identity enabled
- [ ] Monitoring and alerts configured
- [ ] Backup retention configured
- [ ] Geo-replication enabled (if required)
- [ ] Performance baseline established
- [ ] Disaster recovery plan documented

---

**Architecture Status: Production Ready** ✅









