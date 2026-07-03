using AutoMapper;
using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Application.Features.GlobalMedicines.Commands.CreateGlobalMedicine;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Global.Application.Features.GlobalMedicines.Commands.UpdateGlobalMedicine;

public class UpdateGlobalMedicineHandler : IRequestHandler<UpdateGlobalMedicineCommand, Result<GlobalMedicineDto>>
{
    private readonly IGlobalDbContext _context;
    private readonly IMapper _mapper;

    public UpdateGlobalMedicineHandler(IGlobalDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<GlobalMedicineDto>> Handle(UpdateGlobalMedicineCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var medicine = await _context.GlobalMedicines
                .FirstOrDefaultAsync(m => m.Id == request.Id && m.IsActive, cancellationToken);

            if (medicine == null)
                return Result<GlobalMedicineDto>.Failure(new[] { $"Global medicine with ID {request.Id} not found." });

            medicine.Name = request.Name;
            medicine.GenericName = request.GenericName;
            medicine.Type = request.Type;
            medicine.Potency = request.Potency;
            medicine.Manufacturer = request.Manufacturer;
            medicine.Price = request.Price;
            medicine.Description = request.Description ?? string.Empty;
            medicine.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<GlobalMedicineDto>.Success(_mapper.Map<GlobalMedicineDto>(medicine));
        }
        catch (Exception ex)
        {
            return Result<GlobalMedicineDto>.Failure(new[] { $"Failed to update global medicine: {ex.Message}" });
        }
    }
}
