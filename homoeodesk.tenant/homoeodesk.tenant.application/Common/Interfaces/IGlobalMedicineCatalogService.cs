namespace HomoeoDesk.Tenant.Application.Common.Interfaces;

public record GlobalMedicineCatalogItem(
    int Id,
    string Name,
    string GenericName,
    string Type,
    string Potency,
    string Manufacturer,
    decimal Price,
    string Description);

public interface IGlobalMedicineCatalogService
{
    Task<GlobalMedicineCatalogItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
