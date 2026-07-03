using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.ClinicMedicines.Commands.CreateClinicMedicine;
using HomoeoDesk.Tenant.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.ClinicMedicines.Commands.AddClinicMedicineFromGlobal;

public class AddClinicMedicineFromGlobalHandler : IRequestHandler<AddClinicMedicineFromGlobalCommand, Result<ClinicMedicineDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGlobalMedicineCatalogService _globalCatalog;

    public AddClinicMedicineFromGlobalHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IGlobalMedicineCatalogService globalCatalog)
    {
        _context = context;
        _currentUserService = currentUserService;
        _globalCatalog = globalCatalog;
    }

    public async Task<Result<ClinicMedicineDto>> Handle(AddClinicMedicineFromGlobalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
                return Result<ClinicMedicineDto>.Failure(new[] { "User not associated with any organization." });

            var globalMedicine = await _globalCatalog.GetByIdAsync(request.GlobalMedicineId, cancellationToken);
            if (globalMedicine == null)
                return Result<ClinicMedicineDto>.Failure(new[] { "Global medicine not found." });

            var branch = await _context.Branches
                .FirstOrDefaultAsync(b => b.Id == request.BranchId && b.OrganizationId == organizationId.Value, cancellationToken);

            if (branch == null)
                return Result<ClinicMedicineDto>.Failure(new[] { "Branch not found or does not belong to your organization." });

            var exists = await _context.ClinicMedicines.AnyAsync(
                m => m.Name == globalMedicine.Name
                     && m.Potency == globalMedicine.Potency
                     && m.Manufacturer == globalMedicine.Manufacturer,
                cancellationToken);

            if (exists)
                return Result<ClinicMedicineDto>.Failure(new[] { "This medicine already exists in your branch catalog." });

            var medicine = new ClinicMedicine
            {
                OrganizationId = organizationId.Value,
                BranchId = request.BranchId,
                GlobalMedicineId = globalMedicine.Id,
                Name = globalMedicine.Name,
                GenericName = globalMedicine.GenericName,
                Type = globalMedicine.Type,
                Potency = globalMedicine.Potency,
                Manufacturer = globalMedicine.Manufacturer,
                PurchasePrice = request.PurchasePrice ?? globalMedicine.Price,
                SellingPrice = request.SellingPrice ?? globalMedicine.Price,
                Description = globalMedicine.Description,
                IsActive = true
            };

            _context.ClinicMedicines.Add(medicine);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<ClinicMedicineDto>.Success(new ClinicMedicineDto
            {
                Id = medicine.Id,
                BranchId = medicine.BranchId,
                BranchName = branch.Name,
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
            });
        }
        catch (Exception ex)
        {
            return Result<ClinicMedicineDto>.Failure(new[] { $"Failed to add medicine from global catalog: {ex.Message}" });
        }
    }
}
