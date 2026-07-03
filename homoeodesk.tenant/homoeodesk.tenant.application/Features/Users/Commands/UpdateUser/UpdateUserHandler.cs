using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Domain.Entities;
using HomoeoDesk.Tenant.Domain.Enums;
using HomoeoDesk.Tenant.Application.Features.Users.Queries.GetUsers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher _passwordHasher;

    public UpdateUserHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUserService.UserId;
            var currentUserRole = _currentUserService.Role;
            var organizationId = _currentUserService.OrganizationId;

            if (!currentUserId.HasValue || !organizationId.HasValue)
            {
                return Result<UserDto>.Failure("User not authenticated");
            }

            // Only Admin (OrganizationAdmin) can update users
            if (currentUserRole != UserRole.Admin)
            {
                return Result<UserDto>.Failure("Access denied. Only Organization Admin can update users.");
            }

            var user = await _context.Users
                .Include(u => u.DoctorProfile)
                .FirstOrDefaultAsync(u => u.Id == request.Id && u.OrganizationId == organizationId.Value, cancellationToken);

            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            // Check if email is being changed and if it's already taken
            if (user.Email != request.Email)
            {
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == request.Email && u.Id != request.Id && u.OrganizationId == organizationId.Value, cancellationToken);

                if (emailExists)
                {
                    return Result<UserDto>.Failure("A user with this email already exists.");
                }
            }

            // Update user properties
            user.Email = request.Email;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Phone = request.Phone;
            user.Role = request.Role;
            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            // Update password if provided
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = _passwordHasher.HashPassword(request.Password);
            }

            // Update or create doctor profile if role is Doctor
            if (request.Role == UserRole.Doctor)
            {
                if (user.DoctorProfile == null)
                {
                    // Create new doctor profile
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
                else
                {
                    // Update existing doctor profile
                    user.DoctorProfile.RegistrationNumber = request.RegistrationNumber ?? user.DoctorProfile.RegistrationNumber;
                    user.DoctorProfile.Qualification = request.Qualification ?? user.DoctorProfile.Qualification;
                    user.DoctorProfile.Specialization = request.Specialization ?? user.DoctorProfile.Specialization;
                    user.DoctorProfile.ExperienceYears = request.ExperienceYears ?? user.DoctorProfile.ExperienceYears;
                    
                    if (request.ConsultationFeeInPerson.HasValue)
                        user.DoctorProfile.ConsultationFeeInPerson = request.ConsultationFeeInPerson.Value;
                    if (request.ConsultationFeeTele.HasValue)
                        user.DoctorProfile.ConsultationFeeTele = request.ConsultationFeeTele.Value;
                    if (request.FollowupFeeInPerson.HasValue)
                        user.DoctorProfile.FollowupFeeInPerson = request.FollowupFeeInPerson.Value;
                    if (request.FollowupFeeTele.HasValue)
                        user.DoctorProfile.FollowupFeeTele = request.FollowupFeeTele.Value;
                    
                    user.DoctorProfile.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (user.DoctorProfile != null)
            {
                // Remove doctor profile if role changed from Doctor
                _context.DoctorProfiles.Remove(user.DoctorProfile);
            }

            // Update clinic access
            if (request.BranchIds.Any())
            {
                // Remove existing clinic access
                var existingAccess = await _context.UserBranchAccess
                    .Where(uca => uca.UserId == user.Id)
                    .ToListAsync(cancellationToken);

                _context.UserBranchAccess.RemoveRange(existingAccess);

                // Validate clinic IDs
                var validBranches = await _context.Branches
                    .Where(c => request.BranchIds.Contains(c.Id) && c.OrganizationId == organizationId.Value && c.IsActive)
                    .CountAsync(cancellationToken);

                if (validBranches != request.BranchIds.Count)
                {
                    return Result<UserDto>.Failure("One or more clinic IDs are invalid.");
                }

                // Add new clinic access
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

            await _context.SaveChangesAsync(cancellationToken);

            // Reload user with includes
            user = await _context.Users
                .Include(u => u.DoctorProfile)
                .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

            if (user == null)
            {
                return Result<UserDto>.Failure("Failed to retrieve updated user.");
            }

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

            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Failed to update user: {ex.Message}");
        }
    }
}

