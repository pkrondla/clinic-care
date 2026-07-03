using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.ClinicMedicines.Queries.GetClinicMedicines;

public class GetClinicMedicinesHandler : IRequestHandler<GetClinicMedicinesQuery, Result<List<ClinicMedicineDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetClinicMedicinesHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<ClinicMedicineDto>>> Handle(GetClinicMedicinesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<List<ClinicMedicineDto>>.Failure(new[] { "User not associated with any organization." });
            }

            IQueryable<Domain.Entities.ClinicMedicine> query = _context.ClinicMedicines;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.ToLower();
                query = query.Where(m => m.Name.ToLower().Contains(search) ||
                     m.GenericName.ToLower().Contains(search) ||
                     m.Manufacturer.ToLower().Contains(search) ||
                     m.Type.ToLower().Contains(search));
            }

            var medicines = await query
                .OrderBy(m => m.Name)
                .ToListAsync(cancellationToken);

            medicines = medicines.Where(m => m.OrganizationId == organizationId.Value).ToList();

            if (request.IsActive.HasValue)
            {
                medicines = medicines.Where(m => m.IsActive == request.IsActive.Value).ToList();
            }

            if (request.BranchId.HasValue)
            {
                medicines = medicines.Where(m => m.BranchId == request.BranchId.Value).ToList();
            }

            var Branches = new Dictionary<int, string>();

            if (medicines.Count > 0)
            {
                var allBranches = await _context.Branches
                    .Where(c => c.OrganizationId == organizationId.Value)
                    .Select(c => new { c.Id, c.Name })
                    .ToListAsync(cancellationToken);

                var BranchIds = medicines.Select(m => m.BranchId).Distinct().ToHashSet();
                Branches = allBranches
                    .Where(c => BranchIds.Contains(c.Id))
                    .ToDictionary(c => c.Id, c => c.Name);
            }

            var dtos = medicines.Select(m => new ClinicMedicineDto
            {
                Id = m.Id,
                BranchId = m.BranchId,
                BranchName = Branches.GetValueOrDefault(m.BranchId, "Unknown"),
                GlobalMedicineId = m.GlobalMedicineId,
                Name = m.Name,
                GenericName = m.GenericName ?? string.Empty,
                Type = m.Type ?? string.Empty,
                Potency = m.Potency ?? string.Empty,
                Manufacturer = m.Manufacturer ?? string.Empty,
                PurchasePrice = m.PurchasePrice,
                SellingPrice = m.SellingPrice,
                Description = m.Description ?? string.Empty,
                IsActive = m.IsActive,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            }).ToList();

            return Result<List<ClinicMedicineDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<ClinicMedicineDto>>.Failure(new[] { $"Failed to retrieve clinic medicines: {ex.Message}" });
        }
    }
}
