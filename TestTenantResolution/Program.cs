using ClinicCare.Infrastructure;
using ClinicCare.Application;
using ClinicCare.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Add services
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

var host = builder.Build();

using var scope = host.Services.CreateScope();
var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();

try
{
    Console.WriteLine("Testing tenant resolution...");
    var organizationId = await tenantService.GetOrganizationIdAsync();
    Console.WriteLine($"✅ Tenant resolution successful! Organization ID: {organizationId}");
    Console.WriteLine($"✅ Subdomain: {tenantService.Subdomain}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Tenant resolution failed: {ex.Message}");
    Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

