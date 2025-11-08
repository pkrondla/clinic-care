using Microsoft.EntityFrameworkCore;
using ClinicCare.Domain.Tenant;

namespace ClinicCare.Application.Common.Interfaces.Tenant;

public interface ITenantDbContext
{
    // Core entities
    DbSet<Clinic> Clinics { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<Patient> Patients { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<Prescription> Prescriptions { get; }
    DbSet<Medicine> Medicines { get; }

    string TenantId { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}