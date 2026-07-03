using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Doctors.Queries.GetDoctors;

public class GetDoctorsHandler : IRequestHandler<GetDoctorsQuery, Result<List<DoctorDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetDoctorsHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<DoctorDto>>> Handle(GetDoctorsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<List<DoctorDto>>.Failure("User not associated with any organization");
            }

            var query = _context.DoctorProfiles
                .Include(d => d.User)
                .Where(d => d.OrganizationId == organizationId.Value);

            // Filter by active status
            if (request.IsActive.HasValue)
            {
                query = query.Where(d => d.IsActive == request.IsActive.Value);
            }

            // Filter by clinic if specified
            if (request.BranchId.HasValue)
            {
                // Get doctors who have access to this branch
                query = query.Where(d =>
                    _context.UserBranchAccess.Any(uca =>
                        uca.UserId == d.UserId &&
                        uca.BranchId == request.BranchId.Value &&
                        uca.CanAccess &&
                        uca.IsActive));
            }

            var doctors = await query
                .OrderBy(d => d.User.FirstName)
                .ThenBy(d => d.User.LastName)
                .Select(d => new DoctorDto
                {
                    Id = d.Id,
                    UserId = d.UserId,
                    DoctorName = d.User.FullName,
                    Qualification = d.Qualification,
                    Specialization = d.Specialization,
                    RegistrationNumber = d.RegistrationNumber,
                    ExperienceYears = d.ExperienceYears,
                    ConsultationFeeInPerson = d.ConsultationFeeInPerson,
                    ConsultationFeeTele = d.ConsultationFeeTele,
                    FollowupFeeInPerson = d.FollowupFeeInPerson,
                    FollowupFeeTele = d.FollowupFeeTele,
                    IsActive = d.IsActive
                })
                .ToListAsync(cancellationToken);

            return Result<List<DoctorDto>>.Success(doctors);
        }
        catch (Exception ex)
        {
            return Result<List<DoctorDto>>.Failure($"Failed to retrieve doctors: {ex.Message}");
        }
    }
}

