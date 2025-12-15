using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Consultations.Commands.AddConsultationPhoto;

public class AddConsultationPhotoCommand : IRequest<Result<ConsultationPhotoDto>>
{
    public int ConsultationId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ConsultationPhotoDto
{
    public int Id { get; set; }
    public int ConsultationId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

