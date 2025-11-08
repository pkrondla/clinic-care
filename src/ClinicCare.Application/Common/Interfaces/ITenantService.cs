namespace ClinicCare.Application.Common.Interfaces;

public interface ITenantService
{
    string GetTenantId();
    void SetTenantId(string tenantId);
}