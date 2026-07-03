using System.Collections.Generic;

namespace HomoeoDesk.Tenant.Infrastructure.Configuration;

public class TenantSettings
{
    public string DefaultConnectionString { get; set; } = string.Empty;
    public string DatabaseNameTemplate { get; set; } = string.Empty;
    public List<TenantInfo> Tenants { get; set; } = new();
}

public class TenantInfo
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string? ConnectionString { get; set; }
    public bool IsActive { get; set; } = true;
}