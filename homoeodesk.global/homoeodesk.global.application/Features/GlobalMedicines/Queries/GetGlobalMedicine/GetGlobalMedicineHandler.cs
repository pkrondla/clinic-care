using AutoMapper;
using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.GlobalMedicines.Commands.CreateGlobalMedicine;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Global.Application.Features.GlobalMedicines.Queries.GetGlobalMedicine;

public class GetGlobalMedicineHandler : IRequestHandler<GetGlobalMedicineQuery, Result<GlobalMedicineDto>>
{
    private readonly IGlobalDbContext _context;
    private readonly IMapper _mapper;

    public GetGlobalMedicineHandler(IGlobalDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<GlobalMedicineDto>> Handle(GetGlobalMedicineQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var medicine = await _context.GlobalMedicines
                .FirstOrDefaultAsync(m => m.Id == request.Id && m.IsActive, cancellationToken);

            if (medicine == null)
                return Result<GlobalMedicineDto>.Failure(new[] { $"Global medicine with ID {request.Id} not found." });

            return Result<GlobalMedicineDto>.Success(_mapper.Map<GlobalMedicineDto>(medicine));
        }
        catch (Exception ex)
        {
            return Result<GlobalMedicineDto>.Failure(new[] { $"Failed to retrieve global medicine: {ex.Message}" });
        }
    }
}
