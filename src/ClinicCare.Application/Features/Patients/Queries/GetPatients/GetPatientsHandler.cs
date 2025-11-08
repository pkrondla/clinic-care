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

        var query = _context.Patients
            .Include(p => p.User)
            .Include(p => p.Appointments)
            .Include(p => p.Consultations)
            .Where(p => p.OrganizationId == organizationId && p.IsActive);

        // Apply search filter
        if (!string.IsNullOrEmpty(request.Search))
        {
            var searchTerm = request.Search.ToLower();
            query = query.Where(p => 
                p.User.FirstName.ToLower().Contains(searchTerm) ||
                p.User.LastName.ToLower().Contains(searchTerm) ||
                p.User.Email.ToLower().Contains(searchTerm) ||
                p.PatientCode.ToLower().Contains(searchTerm) ||
                p.User.Phone.Contains(searchTerm));
        }

        // Apply gender filter
        if (!string.IsNullOrEmpty(request.Gender))
        {
            query = query.Where(p => p.Gender == request.Gender);
        }

        // Apply blood group filter
        if (!string.IsNullOrEmpty(request.BloodGroup))
        {
            query = query.Where(p => p.BloodGroup == request.BloodGroup);
        }

        // Apply age filters
        if (request.MinAge.HasValue)
        {
            var maxBirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-request.MinAge.Value));
            query = query.Where(p => p.DateOfBirth <= maxBirthDate);
        }

        if (request.MaxAge.HasValue)
        {
            var minBirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-request.MaxAge.Value - 1));
            query = query.Where(p => p.DateOfBirth >= minBirthDate);
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder?.ToLower() == "asc" 
                ? query.OrderBy(p => p.User.FirstName).ThenBy(p => p.User.LastName)
                : query.OrderByDescending(p => p.User.FirstName).ThenByDescending(p => p.User.LastName),
            "email" => request.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(p => p.User.Email)
                : query.OrderByDescending(p => p.User.Email),
            "patientcode" => request.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(p => p.PatientCode)
                : query.OrderByDescending(p => p.PatientCode),
            "age" => request.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(p => p.DateOfBirth)
                : query.OrderByDescending(p => p.DateOfBirth),
            "createdat" => request.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var patients = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PatientDto
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
                Age = p.Age,
                Gender = p.Gender,
                BloodGroup = p.BloodGroup,
                Address = p.Address,
                EmergencyContact = p.EmergencyContact,
                MedicalHistory = p.MedicalHistory,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                IsActive = p.IsActive,
                TotalAppointments = p.Appointments.Count,
                TotalConsultations = p.Consultations.Count,
                LastVisitDate = p.Appointments
                    .Where(a => a.Status == Domain.Enums.AppointmentStatus.Completed)
                    .OrderByDescending(a => a.AppointmentDate)
                    .Select(a => a.AppointmentDate.ToDateTime(TimeOnly.MinValue))
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var result = new PaginatedResult<PatientDto>
        {
            Data = patients,
            Total = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            HasNext = request.Page * request.PageSize < totalCount,
            HasPrevious = request.Page > 1
        };

        return Result<PaginatedResult<PatientDto>>.Success(result);
    }
}
