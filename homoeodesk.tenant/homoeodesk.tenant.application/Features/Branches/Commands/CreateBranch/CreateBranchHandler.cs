using AutoMapper;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Branches.Commands.CreateBranch;

public class CreateBranchHandler : IRequestHandler<CreateBranchCommand, Result<BranchDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public CreateBranchHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<BranchDto>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (organizationId == null)
            {
                return Result<BranchDto>.Failure(new[] { "User is not associated with any organization." });
            }

            var codeExists = await _context.Branches
                .AnyAsync(c => c.Code == request.Code, cancellationToken);

            if (codeExists)
            {
                return Result<BranchDto>.Failure(new[] { $"Branch Code '{request.Code}' already exists." });
            }

            var branch = new Branch
            {
                OrganizationId = organizationId.Value,
                Name = request.Name,
                Code = request.Code,
                Address = request.Address ?? string.Empty,
                ContactPhone = request.Phone ?? string.Empty,
                ContactEmail = request.Email ?? string.Empty,
                IsActive = true
            };

            _context.Branches.Add(branch);
            await _context.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<BranchDto>(branch);

            return Result<BranchDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<BranchDto>.Failure(new[] { $"Failed to create branch: {ex.Message}" });
        }
    }
}
