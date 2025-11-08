using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ClinicCare.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId => GetClaimValue<int>(ClaimTypes.NameIdentifier);
    public string? Email => GetClaimValue<string>(ClaimTypes.Email);
    public UserRole? Role => GetClaimValue<UserRole>("role");
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

    private T? GetClaimValue<T>(string claimType)
    {
        var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
        
        if (string.IsNullOrEmpty(claimValue))
            return default;

        try
        {
            if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
            {
                return (T)(object)int.Parse(claimValue);
            }
            
            if (typeof(T) == typeof(UserRole) || typeof(T) == typeof(UserRole?))
            {
                return (T)(object)Enum.Parse<UserRole>(claimValue);
            }
            
            if (typeof(T) == typeof(string))
            {
                return (T)(object)claimValue;
            }
        }
        catch
        {
            return default;
        }

        return default;
    }
}
