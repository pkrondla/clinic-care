using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Domain.Entities;
using HomoeoDesk.Tenant.Domain.Enums;
using HomoeoDesk.Tenant.Application.Features.Users.Queries.GetUsers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Users.Commands.CreateUser;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _currentUserService.EnsureRole(UserRole.Admin);

        try
        {
            var currentUserId = _currentUserService.UserId;
            var organizationId = _currentUserService.OrganizationId;

            if (!currentUserId.HasValue || !organizationId.HasValue)
            {
                return Result<UserDto>.Failure("User not authenticated");
            }

            // Check if email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.OrganizationId == organizationId.Value, cancellationToken);

            if (existingUser != null)
            {
                return Result<UserDto>.Failure("A user with this email already exists.");
            }

            // Validate clinic IDs
            if (request.BranchIds.Any())
            {
                var validBranches = await _context.Branches
                    .Where(c => request.BranchIds.Contains(c.Id) && c.OrganizationId == organizationId.Value && c.IsActive)
                    .CountAsync(cancellationToken);

                if (validBranches != request.BranchIds.Count)
                {
                    return Result<UserDto>.Failure("One or more clinic IDs are invalid.");
                }
            }

            // Create user
            var user = new User
            {
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                Role = request.Role,
                OrganizationId = organizationId.Value,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            // Create doctor profile if role is Doctor
            if (request.Role == UserRole.Doctor)
            {
                var doctorProfile = new DoctorProfile
                {
                    UserId = user.Id,
                    RegistrationNumber = request.RegistrationNumber ?? string.Empty,
                    Qualification = request.Qualification ?? string.Empty,
                    Specialization = request.Specialization ?? string.Empty,
                    ExperienceYears = request.ExperienceYears ?? 0,
                    ConsultationFeeInPerson = request.ConsultationFeeInPerson ?? 0,
                    ConsultationFeeTele = request.ConsultationFeeTele ?? 0,
                    FollowupFeeInPerson = request.FollowupFeeInPerson ?? 0,
                    FollowupFeeTele = request.FollowupFeeTele ?? 0,
                    OrganizationId = organizationId.Value,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.DoctorProfiles.Add(doctorProfile);
            }

            // Assign clinic access
            if (request.BranchIds.Any())
            {
                foreach (var BranchId in request.BranchIds)
                {
                    var newClinicAccess = new UserBranchAccess
                    {
                        UserId = user.Id,
                        BranchId = BranchId,
                        CanAccess = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.UserBranchAccess.Add(newClinicAccess);
                }
            }
            else
            {
                // If no Branches specified, grant access to all Branches in organization
                var allBranches = await _context.Branches
                    .Where(c => c.OrganizationId == organizationId.Value && c.IsActive)
                    .ToListAsync(cancellationToken);

                foreach (var clinic in allBranches)
                {
                    var newClinicAccess = new UserBranchAccess
                    {
                        UserId = user.Id,
                        BranchId = clinic.Id,
                        CanAccess = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.UserBranchAccess.Add(newClinicAccess);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Reload user with includes
            user = await _context.Users
                .Include(u => u.DoctorProfile)
                .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

            if (user == null)
            {
                return Result<UserDto>.Failure("Failed to retrieve created user.");
            }

            // Get clinic access
            var UserBranchAccess = await _context.UserBranchAccess
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
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                ClinicAccess = UserBranchAccess,
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

            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Failed to create user: {ex.Message}");
        }
    }
}

