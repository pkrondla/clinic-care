namespace ClinicCare.Application.Common.Interfaces;

public interface ITenantDbContextFactory
{
    IApplicationDbContext CreateDbContext();
}