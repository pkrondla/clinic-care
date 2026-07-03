using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Patients.Commands.DeletePatient;

public class DeletePatientCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}

