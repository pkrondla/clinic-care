using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Doctors.Commands.DeleteDoctorAvailability;

public record DeleteDoctorAvailabilityCommand(int Id) : IRequest<Result<Unit>>;

