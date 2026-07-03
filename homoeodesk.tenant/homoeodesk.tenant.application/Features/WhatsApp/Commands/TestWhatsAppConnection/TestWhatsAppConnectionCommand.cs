using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.WhatsApp.Commands.TestWhatsAppConnection;

public class TestWhatsAppConnectionCommand : IRequest<TestWhatsAppConnectionResult>
{
}

public class TestWhatsAppConnectionResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
}

