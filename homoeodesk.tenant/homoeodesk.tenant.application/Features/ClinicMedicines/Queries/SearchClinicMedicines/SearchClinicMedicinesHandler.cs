using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.ClinicMedicines.Queries.SearchClinicMedicines;

public class SearchClinicMedicinesHandler : IRequestHandler<SearchClinicMedicinesQuery, Result<List<ClinicMedicineSearchDto>>>
{
    private readonly IApplicationDbContext _context;

    public SearchClinicMedicinesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ClinicMedicineSearchDto>>> Handle(SearchClinicMedicinesQuery request, CancellationToken cancellationToken)
    {
        try
        {
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

            var dtos = medicines.Select(m => new ClinicMedicineSearchDto
            {
                Id = m.Id,
                Name = m.Name,
                GenericName = m.GenericName ?? string.Empty,
                Manufacturer = m.Manufacturer ?? string.Empty,
                Type = m.Type ?? string.Empty,
                Potency = m.Potency ?? string.Empty
            }).ToList();

            return Result<List<ClinicMedicineSearchDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<ClinicMedicineSearchDto>>.Failure(new[] { $"Failed to search clinic medicines: {ex.Message}" });
        }
    }
}
