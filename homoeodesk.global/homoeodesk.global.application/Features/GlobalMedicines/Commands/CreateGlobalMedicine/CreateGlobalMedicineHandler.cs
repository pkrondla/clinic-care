using AutoMapper;
using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Global.Application.Features.GlobalMedicines.Commands.CreateGlobalMedicine;

public class CreateGlobalMedicineHandler : IRequestHandler<CreateGlobalMedicineCommand, Result<GlobalMedicineDto>>
{
    private readonly IGlobalDbContext _context;
    private readonly IMapper _mapper;

    public CreateGlobalMedicineHandler(IGlobalDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<GlobalMedicineDto>> Handle(CreateGlobalMedicineCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _context.GlobalMedicines.AnyAsync(
                m => m.Name == request.Name && m.Potency == request.Potency && m.Manufacturer == request.Manufacturer,
                cancellationToken);

            if (exists)
                return Result<GlobalMedicineDto>.Failure(new[] { "A medicine with the same name, potency, and manufacturer already exists." });

            var medicine = _mapper.Map<Domain.Entities.GlobalMedicine>(request);
            medicine.CreatedAt = DateTime.UtcNow;
            medicine.UpdatedAt = DateTime.UtcNow;
            medicine.IsActive = true;

            _context.GlobalMedicines.Add(medicine);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<GlobalMedicineDto>.Success(_mapper.Map<GlobalMedicineDto>(medicine));
        }
        catch (Exception ex)
        {
            return Result<GlobalMedicineDto>.Failure(new[] { $"Failed to create global medicine: {ex.Message}" });
        }
    }
}
