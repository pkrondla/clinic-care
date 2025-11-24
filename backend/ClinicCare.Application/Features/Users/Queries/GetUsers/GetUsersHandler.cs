using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Users.Queries.GetUsers;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, Result<List<UserDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUsersHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUserService.UserId;
            var currentUserRole = _currentUserService.Role;
            var organizationId = _currentUserService.OrganizationId;

            if (!currentUserId.HasValue || !organizationId.HasValue)
            {
                return Result<List<UserDto>>.Failure("User not authenticated");
            }

            // Only Admin (OrganizationAdmin) can view all users
            if (currentUserRole != UserRole.Admin)
            {
                return Result<List<UserDto>>.Failure("Access denied. Only Organization Admin can view users.");
            }

            var query = _context.Users
                .Include(u => u.DoctorProfile)
                .Where(u => u.OrganizationId == organizationId.Value)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(u =>
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm) ||
                    u.Phone.Contains(searchTerm));
            }

            if (request.Role.HasValue)
            {
                query = query.Where(u => u.Role == request.Role.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == request.IsActive.Value);
            }

            // Filter by clinic access if specified
            if (request.ClinicId.HasValue)
            {
                query = query.Where(u =>
                    _context.UserClinicAccess.Any(uca =>
                        uca.UserId == u.Id &&
                        uca.ClinicId == request.ClinicId.Value &&
                        uca.CanAccess &&
                        uca.IsActive));
            }

            var users = await query
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync(cancellationToken);

            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                // Get clinic access
                var clinicAccess = await _context.UserClinicAccess
                    .Include(uca => uca.Clinic)
                    .Where(uca => uca.UserId == user.Id && uca.IsActive)
                    .Select(uca => new ClinicAccessDto
                    {
                        ClinicId = uca.ClinicId,
                        ClinicName = uca.Clinic.Name,
                        ClinicCode = uca.Clinic.Code,
                        CanAccess = uca.CanAccess
                    })
                    .ToListAsync(cancellationToken);

                // Get selected clinic name
                string? selectedClinicName = null;
                if (user.SelectedClinicId.HasValue)
                {
                    var clinic = await _context.Clinics
                        .FirstOrDefaultAsync(c => c.Id == user.SelectedClinicId.Value, cancellationToken);
                    selectedClinicName = clinic?.Name;
                }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Phone = user.Phone,
                    Role = user.Role,
                    OrganizationId = user.OrganizationId,
                    OrganizationName = "Organization", // Will be fetched from Global DB if needed
                    SelectedClinicId = user.SelectedClinicId,
                    SelectedClinicName = selectedClinicName,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    ClinicAccess = clinicAccess,
                    DoctorProfile = user.DoctorProfile != null ? new DoctorProfileDto
                    {
                        Id = user.DoctorProfile.Id,
                        Qualification = user.DoctorProfile.Qualification,
                        Specialization = user.DoctorProfile.Specialization,
                        RegistrationNumber = user.DoctorProfile.RegistrationNumber,
                        ExperienceYears = user.DoctorProfile.ExperienceYears,
                        ConsultationFeeInPerson = user.DoctorProfile.ConsultationFeeInPerson,
                        ConsultationFeeTele = user.DoctorProfile.ConsultationFeeTele,
                        FollowupFeeInPerson = user.DoctorProfile.FollowupFeeInPerson,
                        FollowupFeeTele = user.DoctorProfile.FollowupFeeTele,
                        IsActive = user.DoctorProfile.IsActive
                    } : null
                };

                userDtos.Add(userDto);
            }

            return Result<List<UserDto>>.Success(userDtos);
        }
        catch (Exception ex)
        {
            return Result<List<UserDto>>.Failure($"Failed to retrieve users: {ex.Message}");
        }
    }
}

