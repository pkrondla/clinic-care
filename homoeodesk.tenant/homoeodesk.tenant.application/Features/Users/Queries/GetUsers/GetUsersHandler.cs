using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Users.Queries.GetUsers;

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
            var currentUserEmail = _currentUserService.Email;

            // Log for debugging
            Console.WriteLine($"GetUsersHandler: UserId={currentUserId}, Role={currentUserRole}, OrganizationId={organizationId}, Email={currentUserEmail}");

            if (!currentUserId.HasValue || !organizationId.HasValue)
            {
                return Result<List<UserDto>>.Failure("User not authenticated");
            }

            // Only Admin (OrganizationAdmin) can view all users
            // Check if role is Admin (which corresponds to OrganizationAdmin in the UI)
            if (!currentUserRole.HasValue || currentUserRole.Value != UserRole.Admin)
            {
                var roleString = currentUserRole?.ToString() ?? "null";
                var roleInt = currentUserRole?.ToString("D") ?? "null";
                return Result<List<UserDto>>.Failure($"Access denied. Only Organization Admin can view users. Current role: {roleString} (value: {roleInt}), Required: Admin (value: 2)");
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
            if (request.BranchId.HasValue)
            {
                query = query.Where(u =>
                    _context.UserBranchAccess.Any(uca =>
                        uca.UserId == u.Id &&
                        uca.BranchId == request.BranchId.Value &&
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
                var clinicAccess = await _context.UserBranchAccess
                    .Include(uca => uca.Branch)
                    .Where(uca => uca.UserId == user.Id && uca.IsActive)
                    .Select(uca => new BranchAccessDto
                    {
                        BranchId = uca.BranchId,
                        BranchName = uca.Branch.Name,
                        BranchCode = uca.Branch.Code,
                        CanAccess = uca.CanAccess
                    })
                    .ToListAsync(cancellationToken);

                // Get selected Branch Name
                string? SelectedBranchName = null;
                if (user.SelectedBranchId.HasValue)
                {
                    var clinic = await _context.Branches
                        .FirstOrDefaultAsync(c => c.Id == user.SelectedBranchId.Value, cancellationToken);
                    SelectedBranchName = clinic?.Name;
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
                    SelectedBranchId = user.SelectedBranchId,
                    SelectedBranchName = SelectedBranchName,
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

