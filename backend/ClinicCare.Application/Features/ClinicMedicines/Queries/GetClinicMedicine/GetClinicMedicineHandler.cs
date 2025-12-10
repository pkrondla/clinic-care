using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.ClinicMedicines.Queries.GetClinicMedicines;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.ClinicMedicines.Queries.GetClinicMedicine;

public class GetClinicMedicineHandler : IRequestHandler<GetClinicMedicineQuery, Result<ClinicMedicineDto>>
{
    private readonly IClinicMedicineRepository _repository;
    private readonly IApplicationDbContext _context;

    public GetClinicMedicineHandler(IClinicMedicineRepository repository, IApplicationDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<Result<ClinicMedicineDto>> Handle(GetClinicMedicineQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var medicine = await _repository.GetByIdAsync(request.Id, cancellationToken);
            
            if (medicine == null)
            {
                return Result<ClinicMedicineDto>.Failure(new[] { "Clinic medicine not found." });
            }

            var clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == medicine.ClinicId, cancellationToken);

            var dto = new ClinicMedicineDto
            {
                Id = medicine.Id,
                ClinicId = medicine.ClinicId,
                ClinicName = clinic?.Name ?? "Unknown",
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

