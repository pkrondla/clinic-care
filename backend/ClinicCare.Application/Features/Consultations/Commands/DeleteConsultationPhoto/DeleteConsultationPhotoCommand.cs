using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Consultations.Commands.DeleteConsultationPhoto;

public class DeleteConsultationPhotoCommand : IRequest<Result<bool>>
{
    public int PhotoId { get; set; }
}

