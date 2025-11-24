using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Clinics.Commands.CreateClinic;
using MediatR;

namespace ClinicCare.Application.Features.Clinics.Queries.GetClinics;

public class GetClinicsQuery : IRequest<Result<List<ClinicDto>>>
{
    // All clinics for current organization
}

