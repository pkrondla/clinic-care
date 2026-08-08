using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.TrialRequests.Commands.CreateTrialRequest;
using MediatR;

namespace HomoeoDesk.Global.Application.Features.TrialRequests.Queries.GetTrialRequests;

public class GetTrialRequestsQuery : IRequest<Result<List<TrialRequestDto>>>
{
    public string? Status { get; set; }
}
