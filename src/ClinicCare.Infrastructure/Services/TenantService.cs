using ClinicCare.Application.Common.Interfaces;

namespace ClinicCare.Infrastructure.Services;

public class TenantService : ITenantService
{
    private string? _tenantId;

    public string GetTenantId()
    {
        if (string.IsNullOrEmpty(_tenantId))
        {
            throw new InvalidOperationException("TenantId is not set");
        }
        return _tenantId;
    }

    public void SetTenantId(string tenantId)
    {
        _tenantId = tenantId;
    }
}