using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Suppliers.Queries.GetSuppliers;
using HomoeoDesk.Tenant.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Suppliers.Commands.CreateSupplier;

public class CreateSupplierHandler : IRequestHandler<CreateSupplierCommand, Result<SupplierDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateSupplierHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SupplierDto>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<SupplierDto>.Failure("User not associated with any organization");
            }

            // Check if supplier with same name already exists
            var existingSupplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.OrganizationId == organizationId.Value 
                    && s.Name.ToLower() == request.Name.ToLower() 
                    && s.IsActive, cancellationToken);

            if (existingSupplier != null)
            {
                return Result<SupplierDto>.Failure("Supplier with this name already exists");
            }

            var supplier = new Supplier
            {
                OrganizationId = organizationId.Value,
                Name = request.Name,
                ContactPerson = request.ContactPerson,
                Email = request.Email,
                Phone = request.Phone,
                AlternatePhone = request.AlternatePhone,
                Address = request.Address,
                City = request.City,
                State = request.State,
                PinCode = request.PinCode,
                GSTNumber = request.GSTNumber,
                PANNumber = request.PANNumber,
                BankName = request.BankName,
                BankAccountNumber = request.BankAccountNumber,
                IFSCCode = request.IFSCCode,
                Notes = request.Notes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync(cancellationToken);

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
            return Result<SupplierDto>.Failure($"Failed to create supplier: {ex.Message}");
        }
    }
}

