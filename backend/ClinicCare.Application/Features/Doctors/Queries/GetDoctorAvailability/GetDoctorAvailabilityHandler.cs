using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Doctors.Queries.GetDoctorAvailability;

public class GetDoctorAvailabilityHandler : IRequestHandler<GetDoctorAvailabilityQuery, Result<List<DoctorAvailabilityDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetDoctorAvailabilityHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<DoctorAvailabilityDto>>> Handle(GetDoctorAvailabilityQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<List<DoctorAvailabilityDto>>.Failure("User not associated with any organization");
            }

            var query = _context.DoctorAvailabilities
                .Include(da => da.Doctor)
                    .ThenInclude(d => d.User)
                .Include(da => da.Clinic)
                .Where(da => da.OrganizationId == organizationId.Value && da.IsActive);

            if (request.DoctorId.HasValue)
            {
                query = query.Where(da => da.DoctorId == request.DoctorId.Value);
            }

            if (request.ClinicId.HasValue)
            {
                query = query.Where(da => da.ClinicId == request.ClinicId.Value);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(da => da.AvailableDate >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(da => da.AvailableDate <= request.EndDate.Value);
            }

            var availabilities = await query
                .OrderBy(da => da.AvailableDate)
                .ThenBy(da => da.StartTime)
                .Select(da => new DoctorAvailabilityDto
                {
                    Id = da.Id,
                    DoctorId = da.DoctorId,
                    DoctorName = da.Doctor.User.FirstName + " " + da.Doctor.User.LastName,
                    ClinicId = da.ClinicId,
                    ClinicName = da.Clinic.Name,
                    AvailableDate = da.AvailableDate,
                    StartTime = da.StartTime,
                    EndTime = da.EndTime,
                    IsActive = da.IsActive,
                    CreatedAt = da.CreatedAt,
                    UpdatedAt = da.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return Result<List<DoctorAvailabilityDto>>.Success(availabilities);
        }
        catch (Exception ex)
        {
            return Result<List<DoctorAvailabilityDto>>.Failure($"Failed to retrieve doctor availability: {ex.Message}");
        }
    }
}

