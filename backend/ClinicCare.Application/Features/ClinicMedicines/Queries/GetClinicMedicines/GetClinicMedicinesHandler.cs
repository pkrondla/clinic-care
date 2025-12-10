using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.ClinicMedicines.Queries.GetClinicMedicines;

public class GetClinicMedicinesHandler : IRequestHandler<GetClinicMedicinesQuery, Result<List<ClinicMedicineDto>>>
{
    private readonly IClinicMedicineRepository _repository;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetClinicMedicinesHandler(
        IClinicMedicineRepository repository, 
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<ClinicMedicineDto>>> Handle(GetClinicMedicinesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<List<ClinicMedicineDto>>.Failure(new[] { "User not associated with any organization." });
            }

            List<Domain.Entities.ClinicMedicine> medicines;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                medicines = await _repository.SearchAsync(request.SearchTerm, cancellationToken);
            }
            else
            {
                medicines = await _repository.GetAllAsync(cancellationToken);
            }

            // Filter by organization to ensure tenant isolation
            medicines = medicines.Where(m => m.OrganizationId == organizationId.Value).ToList();

            // Filter by IsActive
            // null/undefined = show all, true = active only, false = inactive only
            if (request.IsActive.HasValue)
            {
                medicines = medicines.Where(m => m.IsActive == request.IsActive.Value).ToList();
            }
            // If IsActive is null/undefined, don't filter - show all medicines

            // Filter by clinic if specified
            if (request.ClinicId.HasValue)
            {
                medicines = medicines.Where(m => m.ClinicId == request.ClinicId.Value).ToList();
            }

            // Load clinic names - avoid OPENJSON issues by loading all clinics for organization
            var clinics = new Dictionary<int, string>();
            
            if (organizationId.HasValue && medicines.Count > 0)
            {
                // Load all clinics for the organization and filter in memory
                // This avoids EF Core OPENJSON translation issues
                var allClinics = await _context.Clinics
                    .Where(c => c.OrganizationId == organizationId.Value)
                    .Select(c => new { c.Id, c.Name })
                    .ToListAsync(cancellationToken);
                
                var clinicIds = medicines.Select(m => m.ClinicId).Distinct().ToHashSet();
                clinics = allClinics
                    .Where(c => clinicIds.Contains(c.Id))
                    .ToDictionary(c => c.Id, c => c.Name);
            }

            var dtos = medicines.Select(m => new ClinicMedicineDto
            {
                Id = m.Id,
                ClinicId = m.ClinicId,
                ClinicName = clinics.GetValueOrDefault(m.ClinicId, "Unknown"),
                GlobalMedicineId = m.GlobalMedicineId,
                Name = m.Name,
                GenericName = m.GenericName ?? string.Empty,
                Type = m.Type ?? string.Empty,
                Potency = m.Potency ?? string.Empty,
                Manufacturer = m.Manufacturer ?? string.Empty,
                PurchasePrice = m.PurchasePrice,
                SellingPrice = m.SellingPrice,
                Description = m.Description ?? string.Empty,
                IsActive = m.IsActive,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            }).ToList();

            return Result<List<ClinicMedicineDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<ClinicMedicineDto>>.Failure(new[] { $"Failed to retrieve clinic medicines: {ex.Message}" });
        }
    }
}

