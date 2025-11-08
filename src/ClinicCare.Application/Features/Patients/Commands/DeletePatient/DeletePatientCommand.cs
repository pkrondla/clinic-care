using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Patients.Commands.DeletePatient;

public class DeletePatientCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}

