using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.ClinicMedicines.Commands.DeleteClinicMedicine;

public class DeleteClinicMedicineHandler : IRequestHandler<DeleteClinicMedicineCommand, Result<bool>>
{
    private readonly IClinicMedicineRepository _repository;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteClinicMedicineHandler(
        IClinicMedicineRepository repository,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
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

            // Get medicine without IsActive filter to check for deletion
            var medicine = await _context.ClinicMedicines
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            
            if (medicine == null)
            {
                return Result<bool>.Failure(new[] { "Clinic medicine not found." });
            }

            // Verify medicine belongs to user's organization
            if (medicine.OrganizationId != organizationId.Value)
            {
                return Result<bool>.Failure(new[] { "You do not have permission to delete this medicine." });
            }

            // Check for related records
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
                // Soft delete (set IsActive to false) if related records exist
                medicine.IsActive = false;
                medicine.UpdatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(medicine, cancellationToken);
                return Result<bool>.Success(true);
            }
            else
            {
                // Hard delete if no related records exist
                _context.ClinicMedicines.Remove(medicine);
                await _context.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(new[] { $"Failed to delete clinic medicine: {ex.Message}" });
        }
    }
}

