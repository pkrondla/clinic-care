using ClinicCare.Domain.Entities;

namespace ClinicCare.Application.Common.Interfaces.Tenant;

/// <summary>
/// Repository for Consultation management (Tenant Database)
/// </summary>
public interface IConsultationRepository
{
    Task<Consultation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Consultation?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<Consultation?> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default);
    Task<List<Consultation>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
    Task<List<Consultation>> GetByDoctorIdAsync(int doctorId, DateOnly? date = null, CancellationToken cancellationToken = default);
    Task<Consultation> AddAsync(Consultation consultation, CancellationToken cancellationToken = default);
    Task UpdateAsync(Consultation consultation, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

