using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.WhatsApp.Commands.TestWhatsAppConnection;

public class TestWhatsAppConnectionHandler : IRequestHandler<TestWhatsAppConnectionCommand, TestWhatsAppConnectionResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly WhatsAppProviderFactory _providerFactory;

    public TestWhatsAppConnectionHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        WhatsAppProviderFactory providerFactory)
    {
        _context = context;
        _currentUserService = currentUserService;
        _providerFactory = providerFactory;
    }

    public async Task<TestWhatsAppConnectionResult> Handle(TestWhatsAppConnectionCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.OrganizationId;
        if (!organizationId.HasValue)
        {
            return new TestWhatsAppConnectionResult
            {
                Success = false,
                ErrorMessage = "User not associated with any organization"
            };
        }

        // Check if settings exist
        var settings = await _context.WhatsAppBusinessSettings
            .FirstOrDefaultAsync(
                s => s.OrganizationId == organizationId.Value && s.IsActive,
                cancellationToken);

        if (settings == null)
        {
            return new TestWhatsAppConnectionResult
            {
                Success = false,
                ErrorMessage = "WhatsApp settings not configured. Please configure WhatsApp settings first."
            };
        }

        if (!settings.IsEnabled)
        {
            return new TestWhatsAppConnectionResult
            {
                Success = false,
                ErrorMessage = "WhatsApp is not enabled. Please enable WhatsApp in settings."
            };
        }

        // Get provider and validate configuration
        var provider = await _providerFactory.GetProviderAsync(organizationId.Value, cancellationToken);
        if (provider == null)
        {
            return new TestWhatsAppConnectionResult
            {
                Success = false,
                ErrorMessage = "Failed to create WhatsApp provider. Please check your configuration."
            };
        }

        // Validate configuration
        var isValid = await provider.ValidateConfigurationAsync(cancellationToken);

        return new TestWhatsAppConnectionResult
        {
            Success = isValid,
            Message = isValid 
                ? "WhatsApp connection test successful!" 
                : "WhatsApp connection test failed. Please check your credentials and configuration.",
            ErrorMessage = isValid ? null : "Connection validation failed"
        };
    }
}

