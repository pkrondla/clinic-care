using HomoeoDesk.Tenant.Application.Common.Services;

namespace HomoeoDesk.Tenant.Application.Common.Interfaces;

public interface IWhatsAppProviderFactory
{
    Task<IWhatsAppProviderService?> GetProviderAsync(int organizationId, CancellationToken cancellationToken = default);
}
