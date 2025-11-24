using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Global Tables
    DbSet<GlobalMedicine> GlobalMedicines { get; }
    
    // Tenant Tables
    DbSet<Organization> Organizations { get; }
    DbSet<Clinic> Clinics { get; }
    DbSet<User> Users { get; }
    DbSet<UserOrganization> UserOrganizations { get; }
    DbSet<UserClinicAccess> UserClinicAccess { get; }
    DbSet<DoctorProfile> DoctorProfiles { get; }
    DbSet<DoctorAvailability> DoctorAvailabilities { get; }
    DbSet<Patient> Patients { get; }
    DbSet<Domain.Modules.Appointments.Entities.Appointment> Appointments { get; }
    DbSet<Consultation> Consultations { get; }
    DbSet<Prescription> Prescriptions { get; }
    DbSet<PrescriptionItem> PrescriptionItems { get; }
    DbSet<ClinicMedicine> ClinicMedicines { get; }
    DbSet<Inventory> Inventories { get; }
    DbSet<StockTransaction> StockTransactions { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceItem> InvoiceItems { get; }
    DbSet<Communication> Communications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
