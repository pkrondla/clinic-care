using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Infrastructure.Data.Repositories.Tenant;

public class ConsultationRepository : IConsultationRepository
{
    private readonly TenantDbContext _context;

    public ConsultationRepository(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<Consultation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Consultations
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Consultation?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Consultations
            .Include(c => c.Patient)
                .ThenInclude(p => p!.User)
            .Include(c => c.Doctor)
                .ThenInclude(d => d!.User)
            .Include(c => c.Appointment)
            .Include(c => c.Prescriptions)
                .ThenInclude(p => p.PrescriptionItems)
            .Include(c => c.Photos)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Consultation?> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Consultations
            .Include(c => c.Appointment)
            .Include(c => c.Prescriptions)
            .FirstOrDefaultAsync(c => c.AppointmentId == appointmentId, cancellationToken);
    }

    public async Task<List<Consultation>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return await _context.Consultations
            .Include(c => c.Patient)
                .ThenInclude(p => p!.User)
            .Include(c => c.Doctor)
                .ThenInclude(d => d!.User)
            .Include(c => c.Appointment)
            .Include(c => c.Photos)
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Consultation>> GetByDoctorIdAsync(int doctorId, DateOnly? date = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Consultations
            .Include(c => c.Appointment)
                .ThenInclude(a => a.Patient)
            .Where(c => c.Appointment.DoctorId == doctorId);

        if (date.HasValue)
        {
            query = query.Where(c => c.Appointment.AppointmentDate.Value == date.Value);
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Consultation> AddAsync(Consultation consultation, CancellationToken cancellationToken = default)
    {
        _context.Consultations.Add(consultation);
        await _context.SaveChangesAsync(cancellationToken);
        return consultation;
    }

    public async Task UpdateAsync(Consultation consultation, CancellationToken cancellationToken = default)
    {
        _context.Consultations.Update(consultation);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var consultation = await _context.Consultations.FindAsync(new object[] { id }, cancellationToken);
        if (consultation != null)
        {
            _context.Consultations.Remove(consultation);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

