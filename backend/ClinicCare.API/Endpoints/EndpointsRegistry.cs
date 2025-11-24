using ClinicCare.API.Modules.Appointments;
using ClinicCare.API.Modules.Patients;
using ClinicCare.API.Modules.Inventory;
using ClinicCare.API.Modules.Billing;
using ClinicCare.API.Modules.Payments;
using ClinicCare.API.Modules.Auth;
using ClinicCare.API.Modules.Global;
using ClinicCare.API.Modules.Tenant;
using ClinicCare.API.Modules.Users;
using ClinicCare.API.Modules.Doctors;
using ClinicCare.API.Modules.Reports;

namespace ClinicCare.API.Endpoints;

public static class EndpointsRegistry
{
    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app)
    {
        // Global Admin Endpoints
        app.MapGlobalMedicinesEndpoints();
        app.MapOrganizationsEndpoints();
        
        // Authentication
        app.MapAuthEndpoints();
        
        // Tenant Endpoints
        app.MapClinicsEndpoints();
        app.MapConsultationsEndpoints();
        app.MapPrescriptionsEndpoints();
        app.MapInventoryManagementEndpoints(); // New CQRS-based inventory
        app.MapAppointmentsEndpoints();
        app.MapAppointmentsAdvancedEndpoints();
        app.MapPublicQueueEndpoints(); // Public queue endpoints (no auth)
        app.MapPatientsEndpoints();
        app.MapUsersEndpoints(); // User Management
        app.MapDoctorsEndpoints(); // Doctors
        app.MapSuppliersEndpoints(); // Suppliers
        app.MapPurchaseOrdersEndpoints(); // Purchase Orders
        app.MapStockAuditEndpoints(); // Stock Audit
               // app.MapInventoryEndpoints(); // Legacy placeholder - removed to avoid duplicate endpoint names
               app.MapBillingEndpoints();
               app.MapPaymentWebhookEndpoints(); // Payment webhooks (no auth)
               app.MapReportsEndpoints(); // Reports
               app.MapTestEndpoints(); // Includes password hash generation endpoint

        return app;
    }
}
