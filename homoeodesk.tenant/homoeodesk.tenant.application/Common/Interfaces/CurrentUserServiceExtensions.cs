using HomoeoDesk.Tenant.Domain.Enums;

namespace HomoeoDesk.Tenant.Application.Common.Interfaces;

public static class CurrentUserServiceExtensions
{
    /// <summary>
    /// Throws <see cref="UnauthorizedAccessException"/> if the current user does not have the required role.
    /// Call this before any try/catch that would otherwise swallow the exception as a Result.Failure —
    /// ExceptionMiddleware maps an unhandled UnauthorizedAccessException to a 401.
    /// </summary>
    public static void EnsureRole(this ICurrentUserService currentUser, UserRole role)
    {
        if (!currentUser.IsInRole(role))
            throw new UnauthorizedAccessException($"Access denied. Requires {role} role.");
    }
}
