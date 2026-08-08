using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.TrialRequests.Commands.CreateTrialRequest;
using HomoeoDesk.Global.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Global.Application.Features.TrialRequests.Commands.UpdateTrialRequestStatus;

public class UpdateTrialRequestStatusHandler : IRequestHandler<UpdateTrialRequestStatusCommand, Result<TrialRequestDto>>
{
    private readonly IGlobalDbContext _context;

    public UpdateTrialRequestStatusHandler(IGlobalDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TrialRequestDto>> Handle(UpdateTrialRequestStatusCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TrialRequestStatus>(request.Status, true, out var status))
            return Result<TrialRequestDto>.Failure($"Invalid status '{request.Status}'.");

        var entity = await _context.TrialRequests
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken);

        if (entity is null)
            return Result<TrialRequestDto>.Failure("Trial request not found.");

        entity.Status = status;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<TrialRequestDto>.Success(CreateTrialRequestHandler.Map(entity));
    }
}
