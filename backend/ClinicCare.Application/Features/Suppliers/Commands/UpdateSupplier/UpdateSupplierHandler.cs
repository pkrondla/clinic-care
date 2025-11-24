using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Suppliers.Queries.GetSuppliers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Suppliers.Commands.UpdateSupplier;

public class UpdateSupplierHandler : IRequestHandler<UpdateSupplierCommand, Result<SupplierDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateSupplierHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SupplierDto>> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
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
                    && s.OrganizationId == organizationId.Value, cancellationToken);

            if (supplier == null)
            {
                return Result<SupplierDto>.Failure("Supplier not found");
            }

            // Check if another supplier with same name exists
            var existingSupplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.OrganizationId == organizationId.Value 
                    && s.Id != request.Id
                    && s.Name.ToLower() == request.Name.ToLower() 
                    && s.IsActive, cancellationToken);

            if (existingSupplier != null)
            {
                return Result<SupplierDto>.Failure("Supplier with this name already exists");
            }

            supplier.Name = request.Name;
            supplier.ContactPerson = request.ContactPerson;
            supplier.Email = request.Email;
            supplier.Phone = request.Phone;
            supplier.AlternatePhone = request.AlternatePhone;
            supplier.Address = request.Address;
            supplier.City = request.City;
            supplier.State = request.State;
            supplier.PinCode = request.PinCode;
            supplier.GSTNumber = request.GSTNumber;
            supplier.PANNumber = request.PANNumber;
            supplier.BankName = request.BankName;
            supplier.BankAccountNumber = request.BankAccountNumber;
            supplier.IFSCCode = request.IFSCCode;
            supplier.Notes = request.Notes;
            supplier.IsActive = request.IsActive;
            supplier.UpdatedAt = DateTime.UtcNow;

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
            return Result<SupplierDto>.Failure($"Failed to update supplier: {ex.Message}");
        }
    }
}

