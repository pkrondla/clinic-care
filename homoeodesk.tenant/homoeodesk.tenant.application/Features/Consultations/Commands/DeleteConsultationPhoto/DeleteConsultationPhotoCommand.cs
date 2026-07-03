using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Consultations.Commands.DeleteConsultationPhoto;

public class DeleteConsultationPhotoCommand : IRequest<Result<bool>>
{
    public int PhotoId { get; set; }
}

