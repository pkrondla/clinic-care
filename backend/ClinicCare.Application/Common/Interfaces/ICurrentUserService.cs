using ClinicCare.Domain.Enums;

namespace ClinicCare.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Email { get; }
    UserRole? Role { get; }
    int? OrganizationId { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(UserRole role);
    bool HasAnyRole(params UserRole[] roles);
}
