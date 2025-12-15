using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Patients.Queries.GetPatients;

public class GetPatientsHandler : IRequestHandler<GetPatientsQuery, Result<PaginatedResult<PatientDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public GetPatientsHandler(IApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<PaginatedResult<PatientDto>>> Handle(GetPatientsQuery request, CancellationToken cancellationToken)
    {
        var organizationId = await _tenantService.GetOrganizationIdAsync();

        // Build a simple query using raw SQL for pagination to avoid EF Core OFFSET issues
        var patients = await _context.Patients
            .AsNoTracking()
            .Where(p => p.OrganizationId == organizationId && p.IsActive)
            .Include(p => p.User)
            .Include(p => p.Appointments)
            .OrderByDescending(p => p.Id) // Simple ordering by Id
            .ToListAsync(cancellationToken);

        // Apply search filter in memory if needed
        if (!string.IsNullOrEmpty(request.Search))
        {
            var searchTerm = request.Search.ToLower();
            patients = patients.Where(p => 
                p.User.FirstName.ToLower().Contains(searchTerm) ||
                p.User.LastName.ToLower().Contains(searchTerm) ||
                p.User.Email.ToLower().Contains(searchTerm) ||
                p.PatientCode.ToLower().Contains(searchTerm) ||
                p.User.Phone.Contains(searchTerm)).ToList();
        }

        // Apply gender filter in memory
        if (!string.IsNullOrEmpty(request.Gender))
        {
            patients = patients.Where(p => p.Gender == request.Gender).ToList();
        }

        // Apply blood group filter in memory
        if (!string.IsNullOrEmpty(request.BloodGroup))
        {
            patients = patients.Where(p => p.BloodGroup == request.BloodGroup).ToList();
        }

        // Apply age filters in memory
        if (request.MinAge.HasValue)
        {
            var maxBirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-request.MinAge.Value));
            patients = patients.Where(p => p.DateOfBirth <= maxBirthDate).ToList();
        }

        if (request.MaxAge.HasValue)
        {
            var minBirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-request.MaxAge.Value - 1));
            patients = patients.Where(p => p.DateOfBirth >= minBirthDate).ToList();
        }

        // Get total count after filtering
        var totalCount = patients.Count;

        // Apply sorting in memory
        var sortBy = request.SortBy?.ToLower() ?? "createdat";
        var isAsc = request.SortOrder?.ToLower() == "asc";

        patients = sortBy switch
        {
            "name" => isAsc 
                ? patients.OrderBy(p => p.User.FirstName).ThenBy(p => p.User.LastName).ToList()
                : patients.OrderByDescending(p => p.User.FirstName).ThenByDescending(p => p.User.LastName).ToList(),
            "email" => isAsc
                ? patients.OrderBy(p => p.User.Email).ToList()
                : patients.OrderByDescending(p => p.User.Email).ToList(),
            "patientcode" => isAsc
                ? patients.OrderBy(p => p.PatientCode).ToList()
                : patients.OrderByDescending(p => p.PatientCode).ToList(),
            "age" => isAsc
                ? patients.OrderBy(p => p.DateOfBirth).ToList()
                : patients.OrderByDescending(p => p.DateOfBirth).ToList(),
            _ => isAsc
                ? patients.OrderBy(p => p.CreatedAt).ToList()
                : patients.OrderByDescending(p => p.CreatedAt).ToList()
        };

        // Apply pagination in memory
        var paginatedPatients = patients
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Map to DTOs
        var patientDtos = paginatedPatients.Select(p => 
        {
            var appointments = p.Appointments?.ToList() ?? new List<Domain.Modules.Appointments.Entities.Appointment>();
            var totalAppointments = appointments.Count;
            
            // Get last visit date from completed appointments
            var lastVisitDate = appointments
                .Where(a => a.Status == Domain.Enums.AppointmentStatus.Completed && a.AppointmentDate != null)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => a.AppointmentDate!.Value.ToDateTime(TimeOnly.MinValue))
                .FirstOrDefault();
            
            // Get total consultations from appointments that have consultations
            var totalConsultations = appointments.Count(a => a.Consultation != null);
            
            return new PatientDto
            {
                Id = p.Id,
                UserId = p.UserId,
                PatientCode = p.PatientCode,
                Email = p.User.Email,
                FirstName = p.User.FirstName,
                LastName = p.User.LastName,
                FullName = $"{p.User.FirstName} {p.User.LastName}",
                Phone = p.User.Phone,
                DateOfBirth = p.DateOfBirth,
                Age = DateTime.Now.Year - p.DateOfBirth.Year,
                Gender = p.Gender,
                BloodGroup = p.BloodGroup,
                Address = p.Address,
                EmergencyContact = p.EmergencyContact,
                MedicalHistory = p.MedicalHistory,
                PhotoUrl = p.PhotoUrl,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                IsActive = p.IsActive,
                TotalAppointments = totalAppointments,
                TotalConsultations = totalConsultations,
                LastVisitDate = lastVisitDate != default ? lastVisitDate : null
            };
        }).ToList();

        var result = new PaginatedResult<PatientDto>
        {
            Data = patientDtos,
            Total = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            HasNext = request.Page * request.PageSize < totalCount,
            HasPrevious = request.Page > 1
        };

        return Result<PaginatedResult<PatientDto>>.Success(result);
    }
}
