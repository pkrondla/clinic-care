using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}

