using AutoMapper;
using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Consultations.Commands.CreateConsultation;
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
                .Where(c => c.OrganizationId == organizationId.Value && c.IsActive);

            // Apply filters
            if (request.ClinicId.HasValue)
            {
                query = query.Where(c => c.Appointment != null && c.Appointment.ClinicId == request.ClinicId.Value);
            }

            if (request.DoctorId.HasValue)
            {
                query = query.Where(c => c.DoctorId == request.DoctorId.Value);
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
                CreatedAt = c.CreatedAt
            }).ToList();

            return Result<List<ConsultationDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<ConsultationDto>>.Failure($"Failed to retrieve consultations: {ex.Message}");
        }
    }
}

