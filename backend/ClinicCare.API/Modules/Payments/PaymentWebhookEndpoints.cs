using ClinicCare.Application.Features.Invoices.Commands.ProcessPaymentWebhook;
using MediatR;

namespace ClinicCare.API.Modules.Payments;

public static class PaymentWebhookEndpoints
{
    public static IEndpointRouteBuilder MapPaymentWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        // Payment Webhook (no auth required - gateway calls this)
        app.MapPost("/api/payments/webhook", ProcessPaymentWebhook)
            .WithName("ProcessPaymentWebhook")
            .WithTags("Payments")
            .WithSummary("Process payment webhook from payment gateway")
            .AllowAnonymous()
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> ProcessPaymentWebhook(
        ProcessPaymentWebhookCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { message = "Failed to process payment webhook", errors = result.Errors });
        }

        return Results.Ok(new { message = "Webhook processed successfully", data = result.Data });
    }
}

