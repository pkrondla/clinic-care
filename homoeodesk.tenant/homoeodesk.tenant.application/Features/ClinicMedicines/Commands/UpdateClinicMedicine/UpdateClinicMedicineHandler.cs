using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.ClinicMedicines.Commands.CreateClinicMedicine;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.ClinicMedicines.Commands.UpdateClinicMedicine;

public class UpdateClinicMedicineHandler : IRequestHandler<UpdateClinicMedicineCommand, Result<ClinicMedicineDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateClinicMedicineHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ClinicMedicineDto>> Handle(UpdateClinicMedicineCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<ClinicMedicineDto>.Failure(new[] { "User not associated with any organization." });
            }

            var medicine = await _context.ClinicMedicines
                .FirstOrDefaultAsync(m => m.Id == request.Id && m.IsActive, cancellationToken);

            if (medicine == null)
            {
                return Result<ClinicMedicineDto>.Failure(new[] { "Clinic medicine not found." });
            }

            if (medicine.OrganizationId != organizationId.Value)
            {
                return Result<ClinicMedicineDto>.Failure(new[] { "You do not have permission to update this medicine." });
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
                medicine.Name = request.Name;
            if (!string.IsNullOrWhiteSpace(request.GenericName))
                medicine.GenericName = request.GenericName;
            if (!string.IsNullOrWhiteSpace(request.Type))
                medicine.Type = request.Type;
            if (!string.IsNullOrWhiteSpace(request.Potency))
                medicine.Potency = request.Potency;
            if (!string.IsNullOrWhiteSpace(request.Manufacturer))
                medicine.Manufacturer = request.Manufacturer;
            if (request.PurchasePrice.HasValue)
                medicine.PurchasePrice = request.PurchasePrice.Value;
            if (request.SellingPrice.HasValue)
                medicine.SellingPrice = request.SellingPrice.Value;
            if (request.Description != null)
                medicine.Description = request.Description;
            if (request.IsActive.HasValue)
                medicine.IsActive = request.IsActive.Value;

            medicine.UpdatedAt = DateTime.UtcNow;

            _context.ClinicMedicines.Update(medicine);
            await _context.SaveChangesAsync(cancellationToken);

            var clinic = await _context.Branches.FirstOrDefaultAsync(c => c.Id == medicine.BranchId, cancellationToken);

            var dto = new ClinicMedicineDto
            {
                Id = medicine.Id,
                BranchId = medicine.BranchId,
                BranchName = clinic?.Name ?? "Unknown",
                GlobalMedicineId = medicine.GlobalMedicineId,
                Name = medicine.Name,
                GenericName = medicine.GenericName,
                Type = medicine.Type,
                Potency = medicine.Potency,
                Manufacturer = medicine.Manufacturer,
                PurchasePrice = medicine.PurchasePrice,
                SellingPrice = medicine.SellingPrice,
                Description = medicine.Description,
                IsActive = medicine.IsActive,
                CreatedAt = medicine.CreatedAt,
                UpdatedAt = medicine.UpdatedAt
            };

            return Result<ClinicMedicineDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ClinicMedicineDto>.Failure(new[] { $"Failed to update clinic medicine: {ex.Message}" });
        }
    }
}
