using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Tenant.Application.Features.Authentication.Commands;

public class LoginCommand : IRequest<Result<LoginResponse>>
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public int? BranchId { get; set; }
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfo User { get; set; } = new();
    public List<BranchInfo> AvailableBranches { get; set; } = new();
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
    public int? SelectedBranchId { get; set; }
    public string? SelectedBranchName { get; set; }
}

public class BranchInfo
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
            int organizationId;
            Domain.Entities.User user;

            try
            {
                organizationId = await _tenantService.GetOrganizationIdAsync();

                user = await _context.Users
                    .Include(x => x.DoctorProfile)
                    .FirstOrDefaultAsync(x => x.Email == request.Email && x.OrganizationId == organizationId && x.IsActive, cancellationToken);
            }
            catch
            {
                user = await _context.Users
                    .Include(x => x.DoctorProfile)
                    .FirstOrDefaultAsync(x => x.Email == request.Email && x.IsActive, cancellationToken);

                if (user == null)
                    return Result<LoginResponse>.Failure("Invalid email or password");

                organizationId = user.OrganizationId;
            }

            if (user == null)
                return Result<LoginResponse>.Failure("Invalid email or password");

            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
                return Result<LoginResponse>.Failure("Invalid email or password");

            var AvailableBranches = await GetAvailableBranchesAsync(user.Id, organizationId, cancellationToken);

            int? SelectedBranchId = null;
            string? SelectedBranchName = null;

            if (request.BranchId.HasValue)
            {
                var selectedClinic = AvailableBranches.FirstOrDefault(x => x.Id == request.BranchId.Value);
                if (selectedClinic != null)
                {
                    SelectedBranchId = selectedClinic.Id;
                    SelectedBranchName = selectedClinic.Name;
                }
            }
            else if (user.SelectedBranchId.HasValue)
            {
                var savedClinic = AvailableBranches.FirstOrDefault(x => x.Id == user.SelectedBranchId.Value);
                if (savedClinic != null)
                {
                    SelectedBranchId = savedClinic.Id;
                    SelectedBranchName = savedClinic.Name;
                }
            }

            if (!SelectedBranchId.HasValue && AvailableBranches.Count == 1)
            {
                SelectedBranchId = AvailableBranches[0].Id;
                SelectedBranchName = AvailableBranches[0].Name;
                user.SelectedBranchId = SelectedBranchId;
                await _context.SaveChangesAsync(cancellationToken);
            }

            var organizationName = AvailableBranches.FirstOrDefault()?.Name ?? $"Tenant {organizationId}";

            var (accessToken, refreshToken, expiresAt) = await _tokenService.GenerateTokensAsync(user, SelectedBranchId);

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
                    SelectedBranchId = SelectedBranchId,
                    SelectedBranchName = SelectedBranchName
                },
                AvailableBranches = AvailableBranches
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

    private async Task<List<BranchInfo>> GetAvailableBranchesAsync(int userId, int organizationId, CancellationToken cancellationToken)
    {
        // Check if user has explicit clinic access mappings via UserBranchAccess
        var hasExplicitAccess = await _context.UserBranchAccess
            .AnyAsync(x => x.UserId == userId && x.CanAccess && x.IsActive, cancellationToken);

        // If user has explicit clinic access mappings, use those
        if (hasExplicitAccess)
        {
            // Use a join instead of Contains to avoid OPENJSON issues with SQL Server
            return await (from clinic in _context.Branches
                         join access in _context.UserBranchAccess
                             on clinic.Id equals access.BranchId
                         where access.UserId == userId 
                             && access.CanAccess 
                             && access.IsActive
                             && clinic.OrganizationId == organizationId 
                             && clinic.IsActive
                         select new BranchInfo
                         {
                             Id = clinic.Id,
                             Name = clinic.Name,
                             Code = clinic.Code
                         })
                         .Distinct()
                         .ToListAsync(cancellationToken);
        }

        // Fallback: For doctors, show Branches they have availability for
        // For other roles, show all active Branches in the organization
        var user = await _context.Users
            .Include(x => x.DoctorProfile)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user?.Role == UserRole.Doctor && user.DoctorProfile != null)
        {
            // Use a join instead of Contains to avoid OPENJSON issues
            return await (from clinic in _context.Branches
                         join availability in _context.DoctorAvailabilities
                             on clinic.Id equals availability.BranchId
                         where availability.DoctorId == user.DoctorProfile.Id
                             && availability.AvailableDate >= DateOnly.FromDateTime(DateTime.Today)
                             && clinic.OrganizationId == organizationId
                             && clinic.IsActive
                         select new BranchInfo
                         {
                             Id = clinic.Id,
                             Name = clinic.Name,
                             Code = clinic.Code
                         })
                         .Distinct()
                         .ToListAsync(cancellationToken);
        }

        // Default: Return all active Branches in the organization
        return await _context.Branches
            .Where(x => x.OrganizationId == organizationId && x.IsActive)
            .Select(x => new BranchInfo
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code
            })
            .ToListAsync(cancellationToken);
    }
}
