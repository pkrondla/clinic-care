# Doctor Schedule & Clinic Operating Hours - Phase 1 Backend Implementation

## ✅ Completed

### 1. Database Schema
- ✅ Added clinic operating hours columns (Morning/Evening/FullDay timings)
- ✅ Added `BaseClinicId` to DoctorProfiles  
- ✅ Added `AvailabilityType`, `Notes`, `EndDate` to DoctorAvailabilities
- ✅ Scripts executed successfully

### 2. Domain Enums
- ✅ Created `OperatingHoursType` enum (SingleShift/SplitShift)
- ✅ Created `AvailabilityType` enum (Regular/DifferentClinic/Leave/ModifiedHours)

### 3. Domain Entities
- ✅ Updated `Clinic` entity with operating hours properties
- ✅ Updated `DoctorProfile` entity with `BaseClinicId` and `BaseClinic` navigation
- ✅ Updated `DoctorAvailability` entity with `AvailabilityType`, `Notes`, `EndDate`

## 🔄 Next Steps - Backend Configuration

### 4. Update Entity Configurations

**File: `backend/ClinicCare.Infrastructure/Data/Configurations/ClinicConfiguration.cs`**
```csharp
// Add operating hours property mappings
builder.Property(x => x.OperatingHoursType)
    .HasConversion<int>()
    .IsRequired();

builder.Property(x => x.MorningStartTime)
    .HasColumnType("time");
    
builder.Property(x => x.MorningEndTime)
    .HasColumnType("time");
    
builder.Property(x => x.EveningStartTime)
    .HasColumnType("time");
    
builder.Property(x => x.EveningEndTime)
    .HasColumnType("time");
    
builder.Property(x => x.FullDayStartTime)
    .HasColumnType("time");
    
builder.Property(x => x.FullDayEndTime)
    .HasColumnType("time");
```

**File: `backend/ClinicCare.Infrastructure/Data/Configurations/DoctorProfileConfiguration.cs`**
```csharp
// Add base clinic relationship
builder.HasOne(x => x.BaseClinic)
    .WithMany()
    .HasForeignKey(x => x.BaseClinicId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired(false);
```

**File: `backend/ClinicCare.Infrastructure/Data/Configurations/DoctorAvailabilityConfiguration.cs`**
```csharp
// Add new property mappings
builder.Property(x => x.AvailabilityType)
    .HasConversion<int>()
    .IsRequired();

builder.Property(x => x.EndDate)
    .HasColumnType("date")
    .IsRequired(false);

builder.Property(x => x.Notes)
    .HasMaxLength(500)
    .IsRequired(false);
```

### 5. Update DTOs

**New/Updated DTOs needed:**

1. **ClinicDto** - Add operating hours
2. **DoctorProfileDto** - Add baseClinicId and baseClinicName
3. **DoctorAvailabilityDto** - Add availabilityType, notes, endDate
4. **CreateClinicCommand** - Add operating hours
5. **UpdateClinicCommand** - Add operating hours
6. **CreateDoctorAvailabilityCommand** - Add new fields
7. **UpdateDoctorAvailabilityCommand** - Add new fields

### 6. Backend Services & Endpoints

**Services to update:**
1. Clinic Service - Handle operating hours CRUD
2. Doctor Service - Handle base clinic assignment
3. DoctorAvailability Service - Handle availability types

**New Business Logic Needed:**
- Calculate default availability from base clinic operating hours
- Validate availability entries against clinic hours
- Handle leave date ranges
- Check doctor availability for appointment booking

### 7. Build and Test

1. Update entity configurations (see above)
2. Build backend: `dotnet build backend/ClinicCare.API/ClinicCare.API.csproj`
3. Restart backend
4. Test API endpoints with Postman/Swagger

## 📝 Implementation Notes

### Availability Logic:
1. **Regular (Default)**: Doctor is available at base clinic during clinic's operating hours
2. **Different Clinic**: Doctor is at a different clinic for specified date/time
3. **Leave**: Doctor is not available for date range (AvailableDate to EndDate)
4. **Modified Hours**: Doctor has custom hours for specific date (arriving late, leaving early)

### Clinic Operating Hours:
- **SingleShift**: One continuous shift (use FullDayStartTime/EndTime)
- **SplitShift**: Morning and evening shifts (use Morning*/Evening* times)

### Appointment Booking Integration:
- Check doctor's base clinic hours
- Check for leave entries (AvailabilityType = Leave)
- Check for modified hours or different clinic entries
- Calculate available time slots based on the above

## 🎯 Phase 2 Preview - Frontend UI

Next phase will include:
1. Clinic Management - Operating hours configuration
2. Doctor Management - Base clinic assignment
3. Doctor Schedule UI - Redesigned with availability types
4. Appointment Booking - Integrated with schedule checking

