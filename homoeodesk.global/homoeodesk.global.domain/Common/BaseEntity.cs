namespace HomoeoDesk.Global.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

public abstract class TenantEntity : BaseEntity
{
    public int TenantId { get; set; }
}

public abstract class BranchEntity : TenantEntity
{
    public int? BranchId { get; set; }
}
