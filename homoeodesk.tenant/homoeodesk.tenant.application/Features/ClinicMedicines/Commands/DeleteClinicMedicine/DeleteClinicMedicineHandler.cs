using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.ClinicMedicines.Commands.DeleteClinicMedicine;

public class DeleteClinicMedicineHandler : IRequestHandler<DeleteClinicMedicineCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteClinicMedicineHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(DeleteClinicMedicineCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<bool>.Failure(new[] { "User not associated with any organization." });
            }

            var medicine = await _context.ClinicMedicines
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (medicine == null)
            {
                return Result<bool>.Failure(new[] { "Clinic medicine not found." });
            }

            if (medicine.OrganizationId != organizationId.Value)
            {
                return Result<bool>.Failure(new[] { "You do not have permission to delete this medicine." });
            }

            var hasInventory = await _context.Inventories
                .AnyAsync(i => i.MedicineId == request.Id, cancellationToken);

            var hasPrescriptionItems = await _context.PrescriptionItems
                .AnyAsync(pi => pi.MedicineId == request.Id, cancellationToken);

            var hasStockTransactions = await _context.StockTransactions
                .AnyAsync(st => st.MedicineId == request.Id, cancellationToken);

            var hasPurchaseOrderItems = await _context.PurchaseOrderItems
                .AnyAsync(poi => poi.MedicineId == request.Id, cancellationToken);

            bool hasRelatedRecords = hasInventory || hasPrescriptionItems || hasStockTransactions || hasPurchaseOrderItems;

            if (hasRelatedRecords)
            {
                medicine.IsActive = false;
                medicine.UpdatedAt = DateTime.UtcNow;
                _context.ClinicMedicines.Update(medicine);
                await _context.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }

            _context.ClinicMedicines.Remove(medicine);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(new[] { $"Failed to delete clinic medicine: {ex.Message}" });
        }
    }
}
