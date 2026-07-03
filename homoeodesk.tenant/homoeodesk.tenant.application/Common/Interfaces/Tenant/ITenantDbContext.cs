using HomoeoDesk.Tenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Common.Interfaces.Tenant;

/// <summary>
/// Tenant Database Context Interface
/// Handles organization-specific data with complete isolation
/// Database: HomoeoDesk_{TenantId}
/// </summary>
public interface ITenantDbContext
{
    // Branches & Users
    DbSet<Branch> Branches { get; }
    DbSet<User> Users { get; }
    DbSet<UserBranchAccess> UserBranchAccess { get; }
    DbSet<DoctorProfile> DoctorProfiles { get; }
    DbSet<DoctorAvailability> DoctorAvailabilities { get; }
    
    // Patients
    DbSet<Patient> Patients { get; }
    
    // Appointments & Consultations
    DbSet<Domain.Modules.Appointments.Entities.Appointment> Appointments { get; }
    DbSet<Consultation> Consultations { get; }
    
    // Prescriptions
    DbSet<Prescription> Prescriptions { get; }
    DbSet<PrescriptionItem> PrescriptionItems { get; }
    
    // Medicines (Clinic-specific catalog)
    DbSet<ClinicMedicine> ClinicMedicines { get; }
    
    // Inventory
    DbSet<Inventory> Inventories { get; }
    DbSet<StockTransaction> StockTransactions { get; }
    
    // Billing
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceItem> InvoiceItems { get; }
    
    // Communications
    DbSet<Communication> Communications { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

