using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Suppliers.Queries.GetSuppliers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Suppliers.Queries.GetSupplier;

public class GetSupplierHandler : IRequestHandler<GetSupplierQuery, Result<SupplierDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetSupplierHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SupplierDto>> Handle(GetSupplierQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<SupplierDto>.Failure("User not associated with any organization");
            }

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == request.Id 
                    && s.OrganizationId == organizationId.Value 
                    && s.IsActive, cancellationToken);

            if (supplier == null)
            {
                return Result<SupplierDto>.Failure("Supplier not found");
            }

            var dto = new SupplierDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                AlternatePhone = supplier.AlternatePhone,
                Address = supplier.Address,
                City = supplier.City,
                State = supplier.State,
                PinCode = supplier.PinCode,
                GSTNumber = supplier.GSTNumber,
                PANNumber = supplier.PANNumber,
                BankName = supplier.BankName,
                BankAccountNumber = supplier.BankAccountNumber,
                IFSCCode = supplier.IFSCCode,
                Notes = supplier.Notes,
                IsActive = supplier.IsActive,
                CreatedAt = supplier.CreatedAt,
                UpdatedAt = supplier.UpdatedAt
            };

            return Result<SupplierDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<SupplierDto>.Failure($"Failed to retrieve supplier: {ex.Message}");
        }
    }
}

