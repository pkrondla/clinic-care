using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Clinics.Commands.CreateClinic;
using MediatR;

namespace ClinicCare.Application.Features.Clinics.Queries.GetClinic;

public class GetClinicQuery : IRequest<Result<ClinicDto>>
{
    public int Id { get; set; }
}

