using ClinicCare.Application.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicCare.Infrastructure.Services.PaymentGateways;

/// <summary>
/// Factory for creating payment gateway instances
/// </summary>
public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, Type> _gatewayTypes;

    public PaymentGatewayFactory(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _gatewayTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { "Placeholder", typeof(PlaceholderPaymentGateway) },
            // Add more gateway types here as they are implemented
            // { "Razorpay", typeof(RazorpayPaymentGateway) },
            // { "Stripe", typeof(StripePaymentGateway) },
        };
    }

    public IPaymentGateway GetPaymentGateway()
    {
        var gatewayName = _configuration["Payment:Gateway"] ?? "Placeholder";
        return GetPaymentGateway(gatewayName);
    }

    public IPaymentGateway GetPaymentGateway(string gatewayName)
    {
        if (string.IsNullOrWhiteSpace(gatewayName))
        {
            gatewayName = "Placeholder";
        }

        if (!_gatewayTypes.TryGetValue(gatewayName, out var gatewayType))
        {
            throw new InvalidOperationException($"Payment gateway '{gatewayName}' is not registered");
        }

        var gateway = _serviceProvider.GetService(gatewayType) as IPaymentGateway;
        if (gateway == null)
        {
            throw new InvalidOperationException($"Failed to create payment gateway instance for '{gatewayName}'");
        }

        return gateway;
    }
}

