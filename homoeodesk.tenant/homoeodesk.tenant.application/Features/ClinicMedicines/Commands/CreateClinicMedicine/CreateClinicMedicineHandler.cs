using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.ClinicMedicines.Commands.CreateClinicMedicine;

public class CreateClinicMedicineHandler : IRequestHandler<CreateClinicMedicineCommand, Result<ClinicMedicineDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateClinicMedicineHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ClinicMedicineDto>> Handle(CreateClinicMedicineCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<ClinicMedicineDto>.Failure(new[] { "User not associated with any organization." });
            }

            var exists = await _context.ClinicMedicines
                .AnyAsync(m => m.Name == request.Name && m.Potency == request.Potency && m.Manufacturer == request.Manufacturer, cancellationToken);

            if (exists)
            {
                return Result<ClinicMedicineDto>.Failure(new[] { "A medicine with the same name, potency, and manufacturer already exists in this branch." });
            }

            var clinic = await _context.Branches.FirstOrDefaultAsync(c => c.Id == request.BranchId && c.OrganizationId == organizationId.Value, cancellationToken);
            if (clinic == null)
            {
                return Result<ClinicMedicineDto>.Failure(new[] { "Branch not found or does not belong to your organization." });
            }

            var medicine = new ClinicMedicine
            {
                OrganizationId = organizationId.Value,
                BranchId = request.BranchId,
                GlobalMedicineId = request.GlobalMedicineId,
                Name = request.Name,
                GenericName = request.GenericName,
                Type = request.Type,
                Potency = request.Potency,
                Manufacturer = request.Manufacturer,
                PurchasePrice = request.PurchasePrice,
                SellingPrice = request.SellingPrice,
                Description = request.Description ?? string.Empty,
                IsActive = true
            };

            _context.ClinicMedicines.Add(medicine);
            await _context.SaveChangesAsync(cancellationToken);

            var dto = new ClinicMedicineDto
            {
                Id = medicine.Id,
                BranchId = medicine.BranchId,
                BranchName = clinic.Name,
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
            return Result<ClinicMedicineDto>.Failure(new[] { $"Failed to create clinic medicine: {ex.Message}" });
        }
    }
}
