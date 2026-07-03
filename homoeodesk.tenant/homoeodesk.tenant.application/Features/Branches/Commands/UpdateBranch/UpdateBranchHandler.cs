using AutoMapper;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Branches.Commands.CreateBranch;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Branches.Commands.UpdateBranch;

public class UpdateBranchHandler : IRequestHandler<UpdateBranchCommand, Result<BranchDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateBranchHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<BranchDto>> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var clinic = await _context.Branches
                .FirstOrDefaultAsync(c => c.Id == request.Id && c.IsActive, cancellationToken);

            if (clinic == null)
            {
                return Result<BranchDto>.Failure(new[] { $"Clinic with ID {request.Id} not found." });
            }

            clinic.Name = request.Name;
            clinic.Address = request.Address ?? string.Empty;
            clinic.ContactPhone = request.Phone ?? string.Empty;
            clinic.ContactEmail = request.Email ?? string.Empty;

            if (request.IsActive.HasValue)
            {
                clinic.IsActive = request.IsActive.Value;
            }

            _context.Branches.Update(clinic);
            await _context.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<BranchDto>(clinic);

            return Result<BranchDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<BranchDto>.Failure(new[] { $"Failed to update clinic: {ex.Message}" });
        }
    }
}
