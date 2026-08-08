using HomoeoDesk.Global.Api.Modules.Auth;
using HomoeoDesk.Global.Api.Modules.Global;
using HomoeoDesk.Global.Api.Modules.Public;

namespace HomoeoDesk.Global.Api.Endpoints;

public static class EndpointsRegistry
{
    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAuthEndpoints();
        app.MapPublicEndpoints();
        app.MapGlobalMedicinesEndpoints();
        app.MapOrganizationsEndpoints();
        app.MapSubscriptionsEndpoints();
        app.MapTrialRequestsEndpoints();

        return app;
    }
}