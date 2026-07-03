using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Suppliers.Queries.GetSuppliers;

public class GetSuppliersHandler : IRequestHandler<GetSuppliersQuery, Result<List<SupplierDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetSuppliersHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<SupplierDto>>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<List<SupplierDto>>.Failure("User not associated with any organization");
            }

            var query = _context.Suppliers
                .Where(s => s.OrganizationId == organizationId.Value);

            if (request.IsActive.HasValue)
            {
                query = query.Where(s => s.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(s =>
                    s.Name.ToLower().Contains(searchTerm) ||
                    s.ContactPerson.ToLower().Contains(searchTerm) ||
                    s.Email.ToLower().Contains(searchTerm) ||
                    s.Phone.Contains(searchTerm) ||
                    (s.GSTNumber != null && s.GSTNumber.ToLower().Contains(searchTerm)));
            }

            var suppliers = await query
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);

            var dtos = suppliers.Select(s => new SupplierDto
            {
                Id = s.Id,
                Name = s.Name,
                ContactPerson = s.ContactPerson,
                Email = s.Email,
                Phone = s.Phone,
                AlternatePhone = s.AlternatePhone,
                Address = s.Address,
                City = s.City,
                State = s.State,
                PinCode = s.PinCode,
                GSTNumber = s.GSTNumber,
                PANNumber = s.PANNumber,
                BankName = s.BankName,
                BankAccountNumber = s.BankAccountNumber,
                IFSCCode = s.IFSCCode,
                Notes = s.Notes,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            }).ToList();

            return Result<List<SupplierDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<SupplierDto>>.Failure($"Failed to retrieve suppliers: {ex.Message}");
        }
    }
}

