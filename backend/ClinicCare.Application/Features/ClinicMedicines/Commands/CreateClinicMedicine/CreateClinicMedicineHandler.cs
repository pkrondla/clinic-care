using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.ClinicMedicines.Commands.CreateClinicMedicine;

public class CreateClinicMedicineHandler : IRequestHandler<CreateClinicMedicineCommand, Result<ClinicMedicineDto>>
{
    private readonly IClinicMedicineRepository _repository;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateClinicMedicineHandler(
        IClinicMedicineRepository repository,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
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

            // Check if medicine already exists
            var exists = await _repository.ExistsAsync(request.Name, request.Potency, request.Manufacturer, cancellationToken);
            if (exists)
            {
                return Result<ClinicMedicineDto>.Failure(new[] { "A medicine with the same name, potency, and manufacturer already exists in this clinic." });
            }

            // Verify clinic exists and belongs to organization
            var clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == request.ClinicId && c.OrganizationId == organizationId.Value, cancellationToken);
            if (clinic == null)
            {
                return Result<ClinicMedicineDto>.Failure(new[] { "Clinic not found or does not belong to your organization." });
            }

            // Create entity
            var medicine = new ClinicMedicine
            {
                OrganizationId = organizationId.Value,
                ClinicId = request.ClinicId,
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

            // Save to database
            var created = await _repository.AddAsync(medicine, cancellationToken);

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
            return Result<ClinicMedicineDto>.Failure(new[] { $"Failed to create clinic medicine: {ex.Message}" });
        }
    }
}

