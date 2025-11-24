using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Global;
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
    private readonly IGlobalDbContext _globalContext;
    private readonly ITenantService _tenantService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IGlobalDbContext globalContext,
        ITenantService tenantService,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _context = context;
        _globalContext = globalContext;
        _tenantService = tenantService;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // First, check Global SystemUsers for superadmin login
            var systemUser = await _globalContext.SystemUsers
                .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

            if (systemUser != null)
            {
                // Verify password
                if (!_passwordHasher.VerifyPassword(request.Password, systemUser.PasswordHash))
                {
                    return Result<LoginResponse>.Failure("Invalid email or password");
                }

                // Update last login
                systemUser.LastLoginAt = DateTime.UtcNow;
                await _globalContext.SaveChangesAsync(cancellationToken);

                // Generate tokens for system user
                // Create a temporary User-like object for token generation
                var tempUser = new Domain.Entities.User
                {
                    Id = systemUser.Id,
                    Email = systemUser.Email,
                    FirstName = systemUser.FirstName,
                    LastName = systemUser.LastName,
                    Role = (UserRole)systemUser.Role, // Convert int to UserRole enum
                    OrganizationId = 0 // System users don't belong to an organization
                };

                var (systemAccessToken, systemRefreshToken, systemExpiresAt) = await _tokenService.GenerateTokensAsync(tempUser, null);

                var systemResponse = new LoginResponse
                {
                    AccessToken = systemAccessToken,
                    RefreshToken = systemRefreshToken,
                    ExpiresAt = systemExpiresAt,
                    User = new UserInfo
                    {
                        Id = systemUser.Id,
                        Email = systemUser.Email,
                        FirstName = systemUser.FirstName,
                        LastName = systemUser.LastName,
                        FullName = systemUser.FullName,
                        Role = (UserRole)systemUser.Role,
                        OrganizationId = 0,
                        OrganizationName = "System",
                        SelectedClinicId = null,
                        SelectedClinicName = null
                    },
                    AvailableClinics = new List<ClinicInfo>() // System users don't have clinics
                };

                return Result<LoginResponse>.Success(systemResponse);
            }

            // If not a system user, check tenant users
            int organizationId;
            Domain.Entities.User user;
            
            try
            {
                organizationId = await _tenantService.GetOrganizationIdAsync();
                
                // Find user by email within the tenant
                // Note: Don't include Organization - it's in Global DB, not tenant DB
                user = await _context.Users
                    .Include(x => x.DoctorProfile)
                    .FirstOrDefaultAsync(x => x.Email == request.Email && x.OrganizationId == organizationId && x.IsActive, cancellationToken);
            }
            catch
            {
                // If tenant resolution fails, try to find user by email across all organizations
                // This allows login to work even if tenant resolution fails
                // Note: Don't include Organization - it's in Global DB, not tenant DB
                user = await _context.Users
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

            // Determine selected clinic:
            // 1. Use request.ClinicId if provided and valid
            // 2. Use user's SelectedClinicId if it's in available clinics
            // 3. Auto-select if only one clinic available
            // 4. Otherwise, leave null (user must select)
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
            else if (user.SelectedClinicId.HasValue)
            {
                // Check if user's saved clinic is still available
                var savedClinic = availableClinics.FirstOrDefault(x => x.Id == user.SelectedClinicId.Value);
                if (savedClinic != null)
                {
                    selectedClinicId = savedClinic.Id;
                    selectedClinicName = savedClinic.Name;
                }
            }
            
            // Auto-select if only one clinic available
            if (!selectedClinicId.HasValue && availableClinics.Count == 1)
            {
                selectedClinicId = availableClinics[0].Id;
                selectedClinicName = availableClinics[0].Name;
                
                // Update user's selected clinic in database
                user.SelectedClinicId = selectedClinicId;
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Get organization name from Global database (Organization is not in tenant DB)
            var organization = await _globalContext.Organizations
                .FirstOrDefaultAsync(x => x.Id == organizationId, cancellationToken);
            var organizationName = organization?.Name ?? "Unknown Organization";

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
                    OrganizationName = organizationName,
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
        // Check if user has explicit clinic access mappings via UserClinicAccess
        var hasExplicitAccess = await _context.UserClinicAccess
            .AnyAsync(x => x.UserId == userId && x.CanAccess && x.IsActive, cancellationToken);

        // If user has explicit clinic access mappings, use those
        if (hasExplicitAccess)
        {
            // Use a join instead of Contains to avoid OPENJSON issues with SQL Server
            return await (from clinic in _context.Clinics
                         join access in _context.UserClinicAccess
                             on clinic.Id equals access.ClinicId
                         where access.UserId == userId 
                             && access.CanAccess 
                             && access.IsActive
                             && clinic.OrganizationId == organizationId 
                             && clinic.IsActive
                         select new ClinicInfo
                         {
                             Id = clinic.Id,
                             Name = clinic.Name,
                             Code = clinic.Code
                         })
                         .Distinct()
                         .ToListAsync(cancellationToken);
        }

        // Fallback: For doctors, show clinics they have availability for
        // For other roles, show all active clinics in the organization
        var user = await _context.Users
            .Include(x => x.DoctorProfile)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user?.Role == UserRole.Doctor && user.DoctorProfile != null)
        {
            // Use a join instead of Contains to avoid OPENJSON issues
            return await (from clinic in _context.Clinics
                         join availability in _context.DoctorAvailabilities
                             on clinic.Id equals availability.ClinicId
                         where availability.DoctorId == user.DoctorProfile.Id
                             && availability.AvailableDate >= DateOnly.FromDateTime(DateTime.Today)
                             && clinic.OrganizationId == organizationId
                             && clinic.IsActive
                         select new ClinicInfo
                         {
                             Id = clinic.Id,
                             Name = clinic.Name,
                             Code = clinic.Code
                         })
                         .Distinct()
                         .ToListAsync(cancellationToken);
        }

        // Default: Return all active clinics in the organization
        return await _context.Clinics
            .Where(x => x.OrganizationId == organizationId && x.IsActive)
            .Select(x => new ClinicInfo
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code
            })
            .ToListAsync(cancellationToken);
    }
}
