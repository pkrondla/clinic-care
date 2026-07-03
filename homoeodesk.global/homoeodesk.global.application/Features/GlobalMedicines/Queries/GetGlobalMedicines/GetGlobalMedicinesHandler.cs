using AutoMapper;
using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.GlobalMedicines.Commands.CreateGlobalMedicine;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Global.Application.Features.GlobalMedicines.Queries.GetGlobalMedicines;

public class GetGlobalMedicinesHandler : IRequestHandler<GetGlobalMedicinesQuery, Result<List<GlobalMedicineDto>>>
{
    private readonly IGlobalDbContext _context;
    private readonly IMapper _mapper;

    public GetGlobalMedicinesHandler(IGlobalDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<GlobalMedicineDto>>> Handle(GetGlobalMedicinesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.GlobalMedicines.Where(m => m.IsActive).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.ToLower();
                query = query.Where(m =>
                    m.Name.ToLower().Contains(search) ||
                    m.GenericName.ToLower().Contains(search) ||
                    m.Manufacturer.ToLower().Contains(search) ||
                    m.Type.ToLower().Contains(search));
            }
            else if (!string.IsNullOrWhiteSpace(request.Type))
            {
                query = query.Where(m => m.Type == request.Type);
            }
            else if (!string.IsNullOrWhiteSpace(request.Manufacturer))
            {
                query = query.Where(m => m.Manufacturer == request.Manufacturer);
            }

            var medicines = await query.OrderBy(m => m.Name).ToListAsync(cancellationToken);
            return Result<List<GlobalMedicineDto>>.Success(_mapper.Map<List<GlobalMedicineDto>>(medicines));
        }
        catch (Exception ex)
        {
            return Result<List<GlobalMedicineDto>>.Failure(new[] { $"Failed to retrieve global medicines: {ex.Message}" });
        }
    }
}
