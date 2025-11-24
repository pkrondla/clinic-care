using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Enums;
using ClinicCare.Application.Features.Users.Queries.GetUsers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Users.Queries.GetUser;

public class GetUserHandler : IRequestHandler<GetUserQuery, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUserHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
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

            // Only Admin (OrganizationAdmin) can view user details
            if (currentUserRole != UserRole.Admin)
            {
                return Result<UserDto>.Failure("Access denied. Only Organization Admin can view user details.");
            }

            var user = await _context.Users
                .Include(u => u.DoctorProfile)
                .FirstOrDefaultAsync(u => u.Id == request.Id && u.OrganizationId == organizationId.Value, cancellationToken);

            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

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

            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Failed to retrieve user: {ex.Message}");
        }
    }
}

