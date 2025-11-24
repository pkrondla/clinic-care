using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Suppliers.Commands.DeleteSupplier;

public class DeleteSupplierHandler : IRequestHandler<DeleteSupplierCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteSupplierHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<bool>.Failure("User not associated with any organization");
            }

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == request.Id 
                    && s.OrganizationId == organizationId.Value, cancellationToken);

            if (supplier == null)
            {
                return Result<bool>.Failure("Supplier not found");
            }

            // Check if supplier has any purchase orders
            var hasPurchaseOrders = await _context.PurchaseOrders
                .AnyAsync(po => po.SupplierId == request.Id 
                    && po.OrganizationId == organizationId.Value 
                    && po.IsActive, cancellationToken);

            if (hasPurchaseOrders)
            {
                // Soft delete instead of hard delete
                supplier.IsActive = false;
                supplier.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Hard delete if no purchase orders
                _context.Suppliers.Remove(supplier);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to delete supplier: {ex.Message}");
        }
    }
}

