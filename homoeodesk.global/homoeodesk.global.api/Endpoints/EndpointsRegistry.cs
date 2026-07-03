using HomoeoDesk.Global.Api.Modules.Auth;
using HomoeoDesk.Global.Api.Modules.Global;

namespace HomoeoDesk.Global.Api.Endpoints;

public static class EndpointsRegistry
{
    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAuthEndpoints();
        app.MapGlobalMedicinesEndpoints();
        app.MapOrganizationsEndpoints();
        app.MapSubscriptionsEndpoints();

        return app;
    }
}