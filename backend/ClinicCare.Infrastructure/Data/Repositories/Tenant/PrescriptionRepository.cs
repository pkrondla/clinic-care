using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Infrastructure.Data.Repositories.Tenant;

public class PrescriptionRepository : IPrescriptionRepository
{
    private readonly TenantDbContext _context;

    public PrescriptionRepository(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<Prescription?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Prescriptions
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Prescription?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Prescriptions
            .Include(p => p.Consultation)
                .ThenInclude(c => c.Appointment)
                    .ThenInclude(a => a.Patient)
            .Include(p => p.Consultation)
                .ThenInclude(c => c.Appointment)
                    .ThenInclude(a => a.Doctor)
            .Include(p => p.PrescriptionItems)
                .ThenInclude(pi => pi.Medicine)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Prescription?> GetByConsultationIdAsync(int consultationId, CancellationToken cancellationToken = default)
    {
        return await _context.Prescriptions
            .Include(p => p.PrescriptionItems)
                .ThenInclude(pi => pi.Medicine)
            .FirstOrDefaultAsync(p => p.ConsultationId == consultationId, cancellationToken);
    }

    public async Task<List<Prescription>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return await _context.Prescriptions
            .Include(p => p.Consultation)
                .ThenInclude(c => c.Appointment)
            .Where(p => p.Consultation.Appointment.PatientId == patientId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Prescription> AddAsync(Prescription prescription, CancellationToken cancellationToken = default)
    {
        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync(cancellationToken);
        return prescription;
    }

    public async Task UpdateAsync(Prescription prescription, CancellationToken cancellationToken = default)
    {
        _context.Prescriptions.Update(prescription);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var prescription = await _context.Prescriptions.FindAsync(new object[] { id }, cancellationToken);
        if (prescription != null)
        {
            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<string> GeneratePrescriptionNumberAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var prefix = $"RX{today:yyyyMMdd}";
        
        // Get the last prescription number for today
        var lastPrescription = await _context.Prescriptions
            .Where(p => p.PrescriptionNumber.StartsWith(prefix))
            .OrderByDescending(p => p.PrescriptionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastPrescription == null)
        {
            return $"{prefix}0001";
        }

        // Extract the sequence number and increment
        var lastNumber = lastPrescription.PrescriptionNumber.Substring(prefix.Length);
        if (int.TryParse(lastNumber, out var sequence))
        {
            return $"{prefix}{(sequence + 1):D4}";
        }

        return $"{prefix}0001";
    }
}

