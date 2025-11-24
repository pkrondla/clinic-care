using ClinicCare.Domain.Entities;

namespace ClinicCare.Application.Common.Interfaces.Tenant;

/// <summary>
/// Repository for Prescription management (Tenant Database)
/// </summary>
public interface IPrescriptionRepository
{
    Task<Prescription?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Prescription?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<Prescription?> GetByConsultationIdAsync(int consultationId, CancellationToken cancellationToken = default);
    Task<List<Prescription>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
    Task<Prescription> AddAsync(Prescription prescription, CancellationToken cancellationToken = default);
    Task UpdateAsync(Prescription prescription, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<string> GeneratePrescriptionNumberAsync(CancellationToken cancellationToken = default);
}

