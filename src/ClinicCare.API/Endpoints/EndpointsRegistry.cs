using ClinicCare.API.Modules.Appointments;
using ClinicCare.API.Modules.Patients;
using ClinicCare.API.Modules.Inventory;
using ClinicCare.API.Modules.Billing;
using ClinicCare.API.Modules.Auth;

namespace ClinicCare.API.Endpoints;

public static class EndpointsRegistry
{
    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app)
    {
        // Map all module endpoints
        app.MapAuthEndpoints();
        app.MapAppointmentsEndpoints();
        app.MapAppointmentsAdvancedEndpoints();
        app.MapPatientsEndpoints();
        app.MapInventoryEndpoints();
        app.MapBillingEndpoints();
        app.MapTestEndpoints();

        return app;
    }
}
