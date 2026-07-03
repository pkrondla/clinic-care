namespace HomoeoDesk.Tenant.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

public abstract class TenantEntity : BaseEntity
{
    public int TenantId { get; set; }

    /// <summary>Legacy alias for TenantId during HomoeoDesk migration.</summary>
    public int OrganizationId
    {
        get => TenantId;
        set => TenantId = value;
    }
}

public abstract class BranchEntity : TenantEntity
{
    public int? BranchId { get; set; }
}
