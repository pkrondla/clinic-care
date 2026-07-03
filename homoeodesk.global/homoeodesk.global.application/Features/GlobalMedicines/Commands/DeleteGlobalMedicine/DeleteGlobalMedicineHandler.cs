using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Global.Application.Features.GlobalMedicines.Commands.DeleteGlobalMedicine;

public class DeleteGlobalMedicineHandler : IRequestHandler<DeleteGlobalMedicineCommand, Result<bool>>
{
    private readonly IGlobalDbContext _context;

    public DeleteGlobalMedicineHandler(IGlobalDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(DeleteGlobalMedicineCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var medicine = await _context.GlobalMedicines
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (medicine == null)
                return Result<bool>.Failure(new[] { $"Global medicine with ID {request.Id} not found." });

            medicine.IsActive = false;
            medicine.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(new[] { $"Failed to delete global medicine: {ex.Message}" });
        }
    }
}
