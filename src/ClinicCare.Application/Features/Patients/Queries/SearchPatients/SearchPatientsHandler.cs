using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Patients.Queries.SearchPatients;

public class SearchPatientsHandler : IRequestHandler<SearchPatientsQuery, Result<List<PatientSearchDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public SearchPatientsHandler(IApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<List<PatientSearchDto>>> Handle(SearchPatientsQuery request, CancellationToken cancellationToken)
    {
        var organizationId = await _tenantService.GetOrganizationIdAsync();

        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            return Result<List<PatientSearchDto>>.Success(new List<PatientSearchDto>());
        }

        var searchTerm = request.SearchTerm.ToLower();

        var patients = await _context.Patients
            .Include(p => p.User)
            .Include(p => p.Appointments)
            .Where(p => p.OrganizationId == organizationId && 
                       p.IsActive &&
                       (p.User.FirstName.ToLower().Contains(searchTerm) ||
                        p.User.LastName.ToLower().Contains(searchTerm) ||
                        p.User.Email.ToLower().Contains(searchTerm) ||
                        p.PatientCode.ToLower().Contains(searchTerm) ||
                        p.User.Phone.Contains(searchTerm)))
            .OrderBy(p => p.User.FirstName)
            .ThenBy(p => p.User.LastName)
            .Take(request.Limit)
            .Select(p => new PatientSearchDto
            {
                Id = p.Id,
                PatientCode = p.PatientCode,
                FullName = $"{p.User.FirstName} {p.User.LastName}",
                Email = p.User.Email,
                Phone = p.User.Phone,
                Age = p.Age,
                Gender = p.Gender,
                BloodGroup = p.BloodGroup,
                LastVisitDate = p.Appointments
                    .Where(a => a.Status == Domain.Enums.AppointmentStatus.Completed)
                    .OrderByDescending(a => a.AppointmentDate)
                    .Select(a => a.AppointmentDate.ToDateTime(TimeOnly.MinValue))
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return Result<List<PatientSearchDto>>.Success(patients);
    }
}
