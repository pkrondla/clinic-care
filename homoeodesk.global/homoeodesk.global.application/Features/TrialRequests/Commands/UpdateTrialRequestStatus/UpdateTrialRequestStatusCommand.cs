using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.TrialRequests.Commands.CreateTrialRequest;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Global.Application.Features.TrialRequests.Commands.UpdateTrialRequestStatus;

public class UpdateTrialRequestStatusCommand : IRequest<Result<TrialRequestDto>>
{
    public int Id { get; set; }

    [Required]
    public string Status { get; set; } = string.Empty;
}
