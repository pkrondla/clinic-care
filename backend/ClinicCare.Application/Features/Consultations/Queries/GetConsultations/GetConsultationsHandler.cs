using AutoMapper;
using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Consultations.Commands.CreateConsultation;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Consultations.Queries.GetConsultations;

public class GetConsultationsHandler : IRequestHandler<GetConsultationsQuery, Result<List<ConsultationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetConsultationsHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<List<ConsultationDto>>> Handle(GetConsultationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<List<ConsultationDto>>.Failure("User not associated with any organization");
            }

            var query = _context.Consultations
                .Include(c => c.Patient)
                    .ThenInclude(p => p.User)
                .Include(c => c.Doctor)
                    .ThenInclude(d => d.User)
                .Include(c => c.Prescriptions)
                .Where(c => c.OrganizationId == organizationId.Value && c.IsActive);

            // Apply filters
            if (request.ClinicId.HasValue)
            {
                query = query.Where(c => c.Appointment != null && c.Appointment.ClinicId == request.ClinicId.Value);
            }

            if (request.DoctorId.HasValue)
            {
                // Check if the provided DoctorId is actually a UserId (for backward compatibility)
                // If it matches a UserId, look up the actual DoctorProfile.Id
                var doctorProfile = await _context.DoctorProfiles
                    .FirstOrDefaultAsync(d => d.UserId == request.DoctorId.Value || d.Id == request.DoctorId.Value, cancellationToken);
                
                if (doctorProfile != null)
                {
                    query = query.Where(c => c.DoctorId == doctorProfile.Id);
                }
                else
                {
                    // If no doctor profile found, filter by the provided ID (might be a doctor profile ID)
                    query = query.Where(c => c.DoctorId == request.DoctorId.Value);
                }
            }
            else
            {
                // If no DoctorId provided and current user is a doctor, automatically filter by their doctor profile
                var currentUserId = _currentUserService.UserId;
                if (currentUserId.HasValue)
                {
                    var currentUserRole = _currentUserService.Role;
                    if (currentUserRole == UserRole.Doctor)
                    {
                        var currentUserDoctorProfile = await _context.DoctorProfiles
                            .FirstOrDefaultAsync(d => d.UserId == currentUserId.Value, cancellationToken);
                        
                        if (currentUserDoctorProfile != null)
                        {
                            query = query.Where(c => c.DoctorId == currentUserDoctorProfile.Id);
                        }
                    }
                }
            }

            if (request.PatientId.HasValue)
            {
                query = query.Where(c => c.PatientId == request.PatientId.Value);
            }

            if (request.StartDate.HasValue)
            {
                var startDateTime = request.StartDate.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(c => c.ConsultationDate >= startDateTime);
            }

            if (request.EndDate.HasValue)
            {
                var endDateTime = request.EndDate.Value.ToDateTime(TimeOnly.MaxValue);
                query = query.Where(c => c.ConsultationDate <= endDateTime);
            }

            var consultations = await query
                .OrderByDescending(c => c.ConsultationDate)
                .ThenByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);

            var dtos = consultations.Select(c => new ConsultationDto
            {
                Id = c.Id,
                AppointmentId = c.AppointmentId,
                PatientId = c.PatientId,
                PatientName = c.Patient?.User?.FullName ?? "Unknown",
                DoctorId = c.DoctorId,
                DoctorName = c.Doctor?.User?.FullName ?? "Unknown",
                ConsultationDate = c.ConsultationDate,
                ChiefComplaint = c.ChiefComplaint,
                Symptoms = c.Symptoms,
                Examination = c.Examination,
                Diagnosis = c.Diagnosis,
                TreatmentPlan = c.TreatmentPlan,
                Notes = c.Notes,
                ConsultationFee = c.ConsultationFee,
                CreatedAt = c.CreatedAt,
                HasPrescription = c.Prescriptions != null && c.Prescriptions.Any(p => p.IsActive),
                PrescriptionId = c.Prescriptions != null && c.Prescriptions.Any(p => p.IsActive) 
                    ? c.Prescriptions.FirstOrDefault(p => p.IsActive)?.Id 
                    : null
            }).ToList();

            return Result<List<ConsultationDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<ConsultationDto>>.Failure($"Failed to retrieve consultations: {ex.Message}");
        }
    }
}

