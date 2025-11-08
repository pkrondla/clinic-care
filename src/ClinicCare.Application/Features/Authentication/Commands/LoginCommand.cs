using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Authentication.Commands;

public class LoginCommand : IRequest<Result<LoginResponse>>
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public int? ClinicId { get; set; }
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfo User { get; set; } = new();
    public List<ClinicInfo> AvailableClinics { get; set; } = new();
}

public class UserInfo
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public int? SelectedClinicId { get; set; }
    public string? SelectedClinicName { get; set; }
}

public class ClinicInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        IApplicationDbContext context,
        ITenantService tenantService,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _context = context;
        _tenantService = tenantService;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // For login, we need to resolve tenant from the request context
            // This is different from other endpoints that rely on middleware
            int organizationId;
            Domain.Entities.User user;
            
            try
            {
                organizationId = await _tenantService.GetOrganizationIdAsync();
                
                // Find user by email within the tenant
                user = await _context.Users
                    .Include(x => x.Organization)
                    .Include(x => x.DoctorProfile)
                    .FirstOrDefaultAsync(x => x.Email == request.Email && x.OrganizationId == organizationId && x.IsActive, cancellationToken);
            }
            catch (Exception ex)
            {
                // If tenant resolution fails, try to find user by email across all organizations
                // This allows login to work even if tenant resolution fails
                user = await _context.Users
                    .Include(x => x.Organization)
                    .Include(x => x.DoctorProfile)
                    .FirstOrDefaultAsync(x => x.Email == request.Email && x.IsActive, cancellationToken);

                if (user == null)
                {
                    return Result<LoginResponse>.Failure("Invalid email or password");
                }

                organizationId = user.OrganizationId;
            }

            if (user == null)
            {
                return Result<LoginResponse>.Failure("Invalid email or password");
            }

            // Verify password
            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Result<LoginResponse>.Failure("Invalid email or password");
            }

            // Get available clinics for the user
            var availableClinics = await GetAvailableClinicsAsync(user.Id, organizationId, cancellationToken);

            // Validate selected clinic if provided
            int? selectedClinicId = null;
            string? selectedClinicName = null;
            
            if (request.ClinicId.HasValue)
            {
                var selectedClinic = availableClinics.FirstOrDefault(x => x.Id == request.ClinicId.Value);
                if (selectedClinic != null)
                {
                    selectedClinicId = selectedClinic.Id;
                    selectedClinicName = selectedClinic.Name;
                }
            }

            // Generate tokens
            var (accessToken, refreshToken, expiresAt) = await _tokenService.GenerateTokensAsync(user, selectedClinicId);

            var response = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Role = user.Role,
                    OrganizationId = user.OrganizationId,
                    OrganizationName = user.Organization.Name,
                    SelectedClinicId = selectedClinicId,
                    SelectedClinicName = selectedClinicName
                },
                AvailableClinics = availableClinics
            };

            return Result<LoginResponse>.Success(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Result<LoginResponse>.Failure("Invalid tenant");
        }
        catch (Exception ex)
        {
            return Result<LoginResponse>.Failure($"Login failed: {ex.Message}");
        }
    }

    private async Task<List<ClinicInfo>> GetAvailableClinicsAsync(int userId, int organizationId, CancellationToken cancellationToken)
    {
        var query = _context.Clinics
            .Where(x => x.OrganizationId == organizationId && x.IsActive);

        // For doctors, only show clinics they have availability for
        var user = await _context.Users
            .Include(x => x.DoctorProfile)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user?.Role == UserRole.Doctor && user.DoctorProfile != null)
        {
            var doctorClinicIds = await _context.DoctorAvailabilities
                .Where(x => x.DoctorId == user.DoctorProfile.Id && x.AvailableDate >= DateOnly.FromDateTime(DateTime.Today))
                .Select(x => x.ClinicId)
                .Distinct()
                .ToListAsync(cancellationToken);

            query = query.Where(x => doctorClinicIds.Contains(x.Id));
        }

        return await query
            .Select(x => new ClinicInfo
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code
            })
            .ToListAsync(cancellationToken);
    }
}
