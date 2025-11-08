using ClinicCare.Domain.Modules.Appointments.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Common.Interfaces
{
    public interface IAppointmentRepository
    {
        // Basic CRUD operations
        Task<Appointment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Appointment?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Appointment>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Appointment> AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
        Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        // Business-specific queries
        Task<List<Appointment>> GetByDoctorAndDateAsync(int doctorId, DateOnly date, CancellationToken cancellationToken = default);
        Task<List<Appointment>> GetByPatientAndDateAsync(int patientId, DateOnly date, CancellationToken cancellationToken = default);
        Task<List<Appointment>> GetByClinicAndDateAsync(int clinicId, DateOnly date, CancellationToken cancellationToken = default);
        Task<List<Appointment>> GetUpcomingAppointmentsAsync(int patientId, CancellationToken cancellationToken = default);
        Task<List<Appointment>> GetDoctorQueueAsync(int doctorId, int clinicId, DateOnly date, CancellationToken cancellationToken = default);
        
        // Business validation queries
        Task<bool> IsSlotAvailableAsync(int doctorId, int clinicId, DateOnly date, int tokenNumber, CancellationToken cancellationToken = default);
        Task<bool> HasConflictingAppointmentAsync(int doctorId, int clinicId, DateOnly date, int tokenNumber, int? excludeId = null, CancellationToken cancellationToken = default);
        
        // Statistics queries
        Task<int> GetAppointmentCountByStatusAsync(int? clinicId, int? doctorId, int status, CancellationToken cancellationToken = default);
        Task<int> GetAppointmentCountByTypeAsync(int? clinicId, int? doctorId, int type, CancellationToken cancellationToken = default);
        Task<int> GetAppointmentCountByDateRangeAsync(int? clinicId, int? doctorId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
    }
}

