using HomoeoDesk.Tenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace HomoeoDesk.Tenant.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DatabaseFacade Database { get; }

    DbSet<Branch> Branches { get; }
    DbSet<User> Users { get; }
    DbSet<UserBranchAccess> UserBranchAccess { get; }
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
