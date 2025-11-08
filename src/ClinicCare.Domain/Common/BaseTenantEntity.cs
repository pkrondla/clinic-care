using ClinicCare.Domain.Common;

namespace ClinicCare.Domain.Tenant;

public abstract class BaseTenantEntity : BaseEntity
{
    public string TenantId { get; set; } = null!;
}