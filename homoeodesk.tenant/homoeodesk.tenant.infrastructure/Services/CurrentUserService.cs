using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace HomoeoDesk.Tenant.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService> _logger;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public int? UserId => GetClaimValue<int>(ClaimTypes.NameIdentifier);
    public string? Email => GetClaimValue<string>(ClaimTypes.Email);
    public UserRole? Role => GetRoleClaim();
    public int? OrganizationId => GetClaimValue<int>("organizationId");
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(UserRole role)
    {
        return Role == role;
    }

    public bool HasAnyRole(params UserRole[] roles)
    {
        return Role.HasValue && roles.Contains(Role.Value);
    }

    private UserRole? GetRoleClaim()
    {
        // Try multiple claim types in order of preference
        var claimTypes = new[] { "role", ClaimTypes.Role, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" };
        
        foreach (var claimType in claimTypes)
        {
            var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
            
            if (!string.IsNullOrEmpty(claimValue))
            {
                _logger.LogInformation("CurrentUserService: Found role claim with type '{ClaimType}' and value '{ClaimValue}'", claimType, claimValue);
                
                // Try parsing as enum name first (e.g., "Admin")
                if (Enum.TryParse<UserRole>(claimValue, ignoreCase: true, out var enumValue))
                {
                    _logger.LogInformation("CurrentUserService: Successfully parsed role as enum name: {Role} (value: {RoleValue})", enumValue, (int)enumValue);
                    return enumValue;
                }
                
                // Try parsing as integer if it's a number
                if (int.TryParse(claimValue, out var intValue) && Enum.IsDefined(typeof(UserRole), intValue))
                {
                    var parsedRole = (UserRole)intValue;
                    _logger.LogInformation("CurrentUserService: Successfully parsed role as integer: {Role} (value: {RoleValue})", parsedRole, intValue);
                    return parsedRole;
                }
                
                _logger.LogWarning("CurrentUserService: Failed to parse role claim '{ClaimValue}' as UserRole enum. Valid enum values: {ValidValues}", 
                    claimValue, string.Join(", ", Enum.GetNames(typeof(UserRole))));
            }
        }
        
        // If no role claim found, log all available claims for debugging
        var allClaims = _httpContextAccessor.HttpContext?.User?.Claims?.ToList();
        _logger.LogWarning("CurrentUserService: Role claim not found in any expected claim type. Available claims: {Claims}", 
            string.Join(", ", allClaims?.Select(c => $"{c.Type}={c.Value}") ?? new List<string>()));
        
        return default;
    }

    private T? GetClaimValue<T>(string claimType)
    {
        var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
        
        if (string.IsNullOrEmpty(claimValue))
        {
            return default;
        }

        try
        {
            if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
            {
                return (T)(object)int.Parse(claimValue);
            }
            
            if (typeof(T) == typeof(UserRole) || typeof(T) == typeof(UserRole?))
            {
                _logger.LogInformation("CurrentUserService: Parsing role claim value: '{ClaimValue}'", claimValue);
                
                // Try parsing as enum name first (e.g., "Admin")
                if (Enum.TryParse<UserRole>(claimValue, ignoreCase: true, out var enumValue))
                {
                    _logger.LogInformation("CurrentUserService: Successfully parsed role as enum name: {Role} (value: {RoleValue})", enumValue, (int)enumValue);
                    return (T)(object)enumValue;
                }
                
                // Try parsing as integer if it's a number
                if (int.TryParse(claimValue, out var intValue) && Enum.IsDefined(typeof(UserRole), intValue))
                {
                    var parsedRole = (UserRole)intValue;
                    _logger.LogInformation("CurrentUserService: Successfully parsed role as integer: {Role} (value: {RoleValue})", parsedRole, intValue);
                    return (T)(object)parsedRole;
                }
                
                _logger.LogWarning("CurrentUserService: Failed to parse role claim '{ClaimValue}' as UserRole enum. Valid enum values: {ValidValues}", 
                    claimValue, string.Join(", ", Enum.GetNames(typeof(UserRole))));
                return default;
            }
            
            if (typeof(T) == typeof(string))
            {
                return (T)(object)claimValue;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CurrentUserService: Exception parsing claim '{ClaimType}' with value '{ClaimValue}'", claimType, claimValue);
            return default;
        }

        return default;
    }
}
