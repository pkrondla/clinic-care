using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Domain.Modules.Appointments.Entities;
using ClinicCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Infrastructure.Data.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Appointment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<Appointment?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .Include(x => x.Doctor)
                .ThenInclude(x => x.User)
                .Include(x => x.Patient)
                .ThenInclude(x => x.User)
                .Include(x => x.Clinic)
                .Include(x => x.Consultation)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<Appointment>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .Include(x => x.Doctor)
                .ThenInclude(x => x.User)
                .Include(x => x.Patient)
                .ThenInclude(x => x.User)
                .Include(x => x.Clinic)
                .ToListAsync(cancellationToken);
        }

        public async Task<Appointment> AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync(cancellationToken);
            return appointment;
        }

        public async Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var appointment = await GetByIdAsync(id, cancellationToken);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<List<Appointment>> GetByDoctorAndDateAsync(int doctorId, DateOnly date, CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .Include(x => x.Patient)
                .ThenInclude(x => x.User)
                .Where(x => x.DoctorId == doctorId && x.AppointmentDate.Value == date)
                .OrderBy(x => x.TokenNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Appointment>> GetByPatientAndDateAsync(int patientId, DateOnly date, CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .Include(x => x.Doctor)
                .ThenInclude(x => x.User)
                .Include(x => x.Clinic)
                .Where(x => x.PatientId == patientId && x.AppointmentDate.Value == date)
                .OrderBy(x => x.TokenNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Appointment>> GetByClinicAndDateAsync(int clinicId, DateOnly date, CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .Include(x => x.Doctor)
                .ThenInclude(x => x.User)
                .Include(x => x.Patient)
                .ThenInclude(x => x.User)
                .Where(x => x.ClinicId == clinicId && x.AppointmentDate.Value == date)
                .OrderBy(x => x.TokenNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Appointment>> GetUpcomingAppointmentsAsync(int patientId, CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await _context.Appointments
                .Include(x => x.Doctor)
                .ThenInclude(x => x.User)
                .Include(x => x.Clinic)
                .Where(x => x.PatientId == patientId && x.AppointmentDate.Value >= today)
                .OrderBy(x => x.AppointmentDate.Value)
                .ThenBy(x => x.TokenNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Appointment>> GetDoctorQueueAsync(int doctorId, int clinicId, DateOnly date, CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .Include(x => x.Patient)
                .ThenInclude(x => x.User)
                .Where(x => x.DoctorId == doctorId 
                         && x.ClinicId == clinicId 
                         && x.AppointmentDate.Value == date
                         && (x.Status == AppointmentStatus.Scheduled || x.Status == AppointmentStatus.InProgress))
                .OrderBy(x => x.TokenNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsSlotAvailableAsync(int doctorId, int clinicId, DateOnly date, int tokenNumber, CancellationToken cancellationToken = default)
        {
            return !await _context.Appointments
                .AnyAsync(x => x.DoctorId == doctorId 
                            && x.ClinicId == clinicId 
                            && x.AppointmentDate.Value == date 
                            && x.TokenNumber == tokenNumber
                            && x.Status != AppointmentStatus.Cancelled, cancellationToken);
        }

        public async Task<bool> HasConflictingAppointmentAsync(int doctorId, int clinicId, DateOnly date, int tokenNumber, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Appointments
                .Where(x => x.DoctorId == doctorId 
                         && x.ClinicId == clinicId 
                         && x.AppointmentDate.Value == date 
                         && x.TokenNumber == tokenNumber
                         && x.Status != AppointmentStatus.Cancelled);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<int> GetAppointmentCountByStatusAsync(int? clinicId, int? doctorId, int status, CancellationToken cancellationToken = default)
        {
            var query = _context.Appointments.AsQueryable();

            if (clinicId.HasValue)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (doctorId.HasValue)
                query = query.Where(x => x.DoctorId == doctorId.Value);

            return await query.CountAsync(x => (int)x.Status == status, cancellationToken);
        }

        public async Task<int> GetAppointmentCountByTypeAsync(int? clinicId, int? doctorId, int type, CancellationToken cancellationToken = default)
        {
            var query = _context.Appointments.AsQueryable();

            if (clinicId.HasValue)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (doctorId.HasValue)
                query = query.Where(x => x.DoctorId == doctorId.Value);

            return await query.CountAsync(x => (int)x.Type == type, cancellationToken);
        }

        public async Task<int> GetAppointmentCountByDateRangeAsync(int? clinicId, int? doctorId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
        {
            var query = _context.Appointments.AsQueryable();

            if (clinicId.HasValue)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (doctorId.HasValue)
                query = query.Where(x => x.DoctorId == doctorId.Value);

            return await query.CountAsync(x => x.AppointmentDate.Value >= startDate && x.AppointmentDate.Value <= endDate, cancellationToken);
        }
    }
}
