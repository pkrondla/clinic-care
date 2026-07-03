using HomoeoDesk.Tenant.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HomoeoDesk.Tenant.Infrastructure.Services;

public class BranchService : IBranchService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BranchService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? BranchId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            var branchClaim = context.User.FindFirst("BranchId");
            if (branchClaim != null && int.TryParse(branchClaim.Value, out var claimBranchId))
                return claimBranchId;

            if (context.Request.Headers.TryGetValue("X-Branch-Id", out var headerValue)
                && int.TryParse(headerValue.FirstOrDefault(), out var headerBranchId))
                return headerBranchId;

            // Legacy JWT claim during migration
            var legacyClaim = context.User.FindFirst("clinicId");
            if (legacyClaim != null && int.TryParse(legacyClaim.Value, out var legacyBranchId))
                return legacyBranchId;

            return null;
        }
    }
}
