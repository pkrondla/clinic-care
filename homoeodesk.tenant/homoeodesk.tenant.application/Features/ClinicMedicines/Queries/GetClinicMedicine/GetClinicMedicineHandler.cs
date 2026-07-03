using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.ClinicMedicines.Queries.GetClinicMedicines;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.ClinicMedicines.Queries.GetClinicMedicine;

public class GetClinicMedicineHandler : IRequestHandler<GetClinicMedicineQuery, Result<ClinicMedicineDto>>
{
    private readonly IApplicationDbContext _context;

    public GetClinicMedicineHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ClinicMedicineDto>> Handle(GetClinicMedicineQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var medicine = await _context.ClinicMedicines
                .FirstOrDefaultAsync(m => m.Id == request.Id && m.IsActive, cancellationToken);

            if (medicine == null)
            {
                return Result<ClinicMedicineDto>.Failure(new[] { "Clinic medicine not found." });
            }

            var clinic = await _context.Branches.FirstOrDefaultAsync(c => c.Id == medicine.BranchId, cancellationToken);

            var dto = new ClinicMedicineDto
            {
                Id = medicine.Id,
                BranchId = medicine.BranchId,
                BranchName = clinic?.Name ?? "Unknown",
                GlobalMedicineId = medicine.GlobalMedicineId,
                Name = medicine.Name,
                GenericName = medicine.GenericName ?? string.Empty,
                Type = medicine.Type ?? string.Empty,
                Potency = medicine.Potency ?? string.Empty,
                Manufacturer = medicine.Manufacturer ?? string.Empty,
                PurchasePrice = medicine.PurchasePrice,
                SellingPrice = medicine.SellingPrice,
                Description = medicine.Description ?? string.Empty,
                IsActive = medicine.IsActive,
                CreatedAt = medicine.CreatedAt,
                UpdatedAt = medicine.UpdatedAt
            };

            return Result<ClinicMedicineDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ClinicMedicineDto>.Failure(new[] { $"Failed to retrieve clinic medicine: {ex.Message}" });
        }
    }
}
