using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Appointments.Queries.GetAllQueues;

public class GetAllQueuesHandler : IRequestHandler<GetAllQueuesQuery, Result<List<DoctorQueueDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAllQueuesHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<DoctorQueueDto>>> Handle(GetAllQueuesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<List<DoctorQueueDto>>.Failure("User not associated with any organization");
            }

            var date = request.Date ?? DateOnly.FromDateTime(DateTime.Today);
            var clinicId = request.ClinicId;

            // Get all doctors for the clinic/organization
            var doctorsQuery = _context.DoctorProfiles
                .Include(d => d.User)
                .Where(d => d.OrganizationId == organizationId.Value && d.IsActive);

            if (clinicId.HasValue)
            {
                // Filter doctors who have appointments in this clinic on this date
                doctorsQuery = doctorsQuery.Where(d =>
                    _context.Appointments.Any(a =>
                        a.DoctorId == d.Id &&
                        a.ClinicId == clinicId.Value &&
                        a.AppointmentDate.Value == date &&
                        a.IsActive));
            }

            var doctors = await doctorsQuery.ToListAsync(cancellationToken);

            var queues = new List<DoctorQueueDto>();

            foreach (var doctor in doctors)
            {
                // Get all appointments for this doctor on this date
                var appointmentsQuery = _context.Appointments
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.User)
                    .Where(a => a.DoctorId == doctor.Id
                             && a.AppointmentDate.Value == date
                             && a.OrganizationId == organizationId.Value
                             && a.IsActive);

                if (clinicId.HasValue)
                {
                    appointmentsQuery = appointmentsQuery.Where(a => a.ClinicId == clinicId.Value);
                }

                var appointments = await appointmentsQuery
                    .OrderBy(a => a.TokenNumber)
                    .ToListAsync(cancellationToken);

                if (!appointments.Any() && clinicId.HasValue)
                {
                    continue; // Skip doctors with no appointments
                }

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

                queues.Add(new DoctorQueueDto
                {
                    DoctorId = doctor.Id,
                    DoctorName = doctor.User?.FullName ?? "Unknown",
                    Qualification = doctor.Qualification,
                    CurrentToken = currentToken,
                    TotalTokens = appointments.Count,
                    WaitingTokens = waitingTokens,
                    Tokens = tokens
                });
            }

            // Sort by doctor name
            queues = queues.OrderBy(q => q.DoctorName).ToList();

            return Result<List<DoctorQueueDto>>.Success(queues);
        }
        catch (Exception ex)
        {
            return Result<List<DoctorQueueDto>>.Failure($"Failed to retrieve queues: {ex.Message}");
        }
    }
}

