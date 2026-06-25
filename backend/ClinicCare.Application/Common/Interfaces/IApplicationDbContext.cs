using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ClinicCare.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Database facade for transactions
    DatabaseFacade Database { get; }
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
    DbSet<ConsultationPhoto> ConsultationPhotos { get; }
    DbSet<Prescription> Prescriptions { get; }
    DbSet<PrescriptionItem> PrescriptionItems { get; }
    DbSet<ClinicMedicine> ClinicMedicines { get; }
    DbSet<Inventory> Inventories { get; }
    DbSet<StockTransaction> StockTransactions { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceItem> InvoiceItems { get; }
    DbSet<Communication> Communications { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderItem> PurchaseOrderItems { get; }
    DbSet<WhatsAppBusinessSettings> WhatsAppBusinessSettings { get; }
    DbSet<EmailSettings> EmailSettings { get; }
    DbSet<SmsSettings> SmsSettings { get; }
    DbSet<NotificationPreferences> NotificationPreferences { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
