using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Global;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.ClinicMedicines.Commands.CreateClinicMedicine;
using ClinicCare.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.ClinicMedicines.Commands.AddClinicMedicineFromGlobal;

public class AddClinicMedicineFromGlobalHandler : IRequestHandler<AddClinicMedicineFromGlobalCommand, Result<ClinicMedicineDto>>
{
    private readonly IClinicMedicineRepository _clinicMedicineRepository;
    private readonly IGlobalMedicineRepository _globalMedicineRepository;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AddClinicMedicineFromGlobalHandler(
        IClinicMedicineRepository clinicMedicineRepository,
        IGlobalMedicineRepository globalMedicineRepository,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _clinicMedicineRepository = clinicMedicineRepository;
        _globalMedicineRepository = globalMedicineRepository;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ClinicMedicineDto>> Handle(AddClinicMedicineFromGlobalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<ClinicMedicineDto>.Failure(new[] { "User not associated with any organization." });
            }

            // Get global medicine
            var globalMedicine = await _globalMedicineRepository.GetByIdAsync(request.GlobalMedicineId, cancellationToken);
            if (globalMedicine == null)
            {
                return Result<ClinicMedicineDto>.Failure(new[] { "Global medicine not found." });
            }

            // Verify clinic exists and belongs to organization
            var clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == request.ClinicId && c.OrganizationId == organizationId.Value, cancellationToken);
            if (clinic == null)
            {
                return Result<ClinicMedicineDto>.Failure(new[] { "Clinic not found or does not belong to your organization." });
            }

            // Check if already exists in clinic
            var exists = await _clinicMedicineRepository.ExistsAsync(globalMedicine.Name, globalMedicine.Potency, globalMedicine.Manufacturer, cancellationToken);
            if (exists)
            {
                return Result<ClinicMedicineDto>.Failure(new[] { "This medicine already exists in your clinic catalog." });
            }

            // Create clinic medicine from global medicine
            var clinicMedicine = new ClinicMedicine
            {
                OrganizationId = organizationId.Value,
                ClinicId = request.ClinicId,
                GlobalMedicineId = globalMedicine.Id,
                Name = globalMedicine.Name,
                GenericName = globalMedicine.GenericName,
                Type = globalMedicine.Type,
                Potency = globalMedicine.Potency,
                Manufacturer = globalMedicine.Manufacturer,
                PurchasePrice = request.PurchasePrice ?? globalMedicine.Price,
                SellingPrice = request.SellingPrice ?? globalMedicine.Price,
                Description = globalMedicine.Description ?? string.Empty,
                IsActive = true
            };

            var created = await _clinicMedicineRepository.AddAsync(clinicMedicine, cancellationToken);

            // Map to DTO
            var dto = new ClinicMedicineDto
            {
                Id = created.Id,
                ClinicId = created.ClinicId,
                ClinicName = clinic.Name,
                GlobalMedicineId = created.GlobalMedicineId,
                Name = created.Name,
                GenericName = created.GenericName,
                Type = created.Type,
                Potency = created.Potency,
                Manufacturer = created.Manufacturer,
                PurchasePrice = created.PurchasePrice,
                SellingPrice = created.SellingPrice,
                Description = created.Description,
                IsActive = created.IsActive,
                CreatedAt = created.CreatedAt,
                UpdatedAt = created.UpdatedAt
            };

            return Result<ClinicMedicineDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ClinicMedicineDto>.Failure(new[] { $"Failed to add medicine from global catalog: {ex.Message}" });
        }
    }
}

