using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.ClinicMedicines.Commands.CreateClinicMedicine;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.ClinicMedicines.Commands.UpdateClinicMedicine;

public class UpdateClinicMedicineHandler : IRequestHandler<UpdateClinicMedicineCommand, Result<ClinicMedicineDto>>
{
    private readonly IClinicMedicineRepository _repository;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateClinicMedicineHandler(
        IClinicMedicineRepository repository,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
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

            var medicine = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (medicine == null)
            {
                return Result<ClinicMedicineDto>.Failure(new[] { "Clinic medicine not found." });
            }

            // Verify medicine belongs to user's organization
            if (medicine.OrganizationId != organizationId.Value)
            {
                return Result<ClinicMedicineDto>.Failure(new[] { "You do not have permission to update this medicine." });
            }

            // Update properties if provided
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

            await _repository.UpdateAsync(medicine, cancellationToken);

            var clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == medicine.ClinicId, cancellationToken);

            var dto = new ClinicMedicineDto
            {
                Id = medicine.Id,
                ClinicId = medicine.ClinicId,
                ClinicName = clinic?.Name ?? "Unknown",
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

