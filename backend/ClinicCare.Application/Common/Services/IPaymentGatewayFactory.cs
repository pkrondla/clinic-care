namespace ClinicCare.Application.Common.Services;

/// <summary>
/// Factory interface for creating payment gateway instances
/// </summary>
public interface IPaymentGatewayFactory
{
    /// <summary>
    /// Get the configured payment gateway instance
    /// </summary>
    IPaymentGateway GetPaymentGateway();

    /// <summary>
    /// Get a specific payment gateway by name
    /// </summary>
    IPaymentGateway GetPaymentGateway(string gatewayName);
}

