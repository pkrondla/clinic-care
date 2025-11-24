using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Appointments.Queries.GetAllQueues;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Appointments.Queries.GetQueue;

public class GetQueueHandler : IRequestHandler<GetQueueQuery, Result<DoctorQueueDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetQueueHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<DoctorQueueDto>> Handle(GetQueueQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<DoctorQueueDto>.Failure("User not associated with any organization");
            }

            var date = request.Date ?? DateOnly.FromDateTime(DateTime.Today);

            // Get doctor
            var doctor = await _context.DoctorProfiles
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == request.DoctorId 
                                       && d.OrganizationId == organizationId.Value 
                                       && d.IsActive, cancellationToken);

            if (doctor == null)
            {
                return Result<DoctorQueueDto>.Failure("Doctor not found");
            }

            // Get all appointments for this doctor on this date
            var appointmentsQuery = _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Where(a => a.DoctorId == request.DoctorId
                         && a.AppointmentDate.Value == date
                         && a.OrganizationId == organizationId.Value
                         && a.IsActive);

            if (request.ClinicId.HasValue)
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.ClinicId == request.ClinicId.Value);
            }

            var appointments = await appointmentsQuery
                .OrderBy(a => a.TokenNumber)
                .ToListAsync(cancellationToken);

            var tokens = appointments.Select(a => new QueueTokenDto
            {
                TokenNumber = a.TokenNumber,
                AppointmentId = a.Id,
                Status = (int)a.Status,
                StatusText = a.Status switch
                {
                    AppointmentStatus.Scheduled => "Waiting",
                    AppointmentStatus.InProgress => "In Progress",
                    AppointmentStatus.Completed => "Completed",
                    AppointmentStatus.Cancelled => "Cancelled",
                    _ => "Unknown"
                },
                PatientId = request.IncludePatientDetails ? a.PatientId : null,
                PatientName = request.IncludePatientDetails ? a.Patient.User?.FullName : null,
                PatientMobile = request.IncludePatientDetails ? a.Patient.User?.Phone : null,
                PatientCode = request.IncludePatientDetails ? a.Patient.PatientCode : null,
                CreatedAt = a.CreatedAt
            }).ToList();

            var currentToken = appointments
                .Where(a => a.Status == AppointmentStatus.InProgress)
                .OrderBy(a => a.TokenNumber)
                .FirstOrDefault()?.TokenNumber ?? 0;

            var waitingTokens = appointments.Count(a => a.Status == AppointmentStatus.Scheduled);

            var queue = new DoctorQueueDto
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.User?.FullName ?? "Unknown",
                Qualification = doctor.Qualification,
                CurrentToken = currentToken,
                TotalTokens = appointments.Count,
                WaitingTokens = waitingTokens,
                Tokens = tokens
            };

            return Result<DoctorQueueDto>.Success(queue);
        }
        catch (Exception ex)
        {
            return Result<DoctorQueueDto>.Failure($"Failed to retrieve queue: {ex.Message}");
        }
    }
}

