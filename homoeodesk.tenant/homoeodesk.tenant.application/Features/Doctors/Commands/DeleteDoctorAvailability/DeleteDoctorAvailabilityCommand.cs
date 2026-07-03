using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Doctors.Commands.DeleteDoctorAvailability;

public record DeleteDoctorAvailabilityCommand(int Id) : IRequest<Result<Unit>>;

