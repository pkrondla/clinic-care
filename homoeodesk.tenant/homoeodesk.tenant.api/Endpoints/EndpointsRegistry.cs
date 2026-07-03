using HomoeoDesk.Tenant.Api.Modules.Appointments;
using HomoeoDesk.Tenant.Api.Modules.Patients;
using HomoeoDesk.Tenant.Api.Modules.Inventory;
using HomoeoDesk.Tenant.Api.Modules.Billing;
using HomoeoDesk.Tenant.Api.Modules.Payments;
using HomoeoDesk.Tenant.Api.Modules.Auth;
using HomoeoDesk.Tenant.Api.Modules.Tenant;
using HomoeoDesk.Tenant.Api.Modules.Users;
using HomoeoDesk.Tenant.Api.Modules.Doctors;
using HomoeoDesk.Tenant.Api.Modules.Reports;
using HomoeoDesk.Tenant.Api.Modules.Settings;

namespace HomoeoDesk.Tenant.Api.Endpoints;

public static class EndpointsRegistry
{
    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAuthEndpoints();

        app.MapBranchesEndpoints();
        app.MapConsultationsEndpoints();
        app.MapPrescriptionsEndpoints();
        app.MapClinicMedicinesEndpoints();
        app.MapInventoryManagementEndpoints();
        app.MapAppointmentsEndpoints();
        app.MapAppointmentsAdvancedEndpoints();
        app.MapPublicQueueEndpoints();
        app.MapPatientsEndpoints();
        app.MapUsersEndpoints();
        app.MapDoctorsEndpoints();
        app.MapSuppliersEndpoints();
        app.MapPurchaseOrdersEndpoints();
        app.MapStockAuditEndpoints();
        app.MapBillingEndpoints();
        app.MapPaymentWebhookEndpoints();
        app.MapReportsEndpoints();
        app.MapWhatsAppEndpoints();
        app.MapEmailEndpoints();
        app.MapSmsEndpoints();
        app.MapNotificationPreferencesEndpoints();
        app.MapTestEndpoints();

        return app;
    }
}
