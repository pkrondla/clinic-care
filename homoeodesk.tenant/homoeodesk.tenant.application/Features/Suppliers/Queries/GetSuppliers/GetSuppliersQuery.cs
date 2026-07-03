using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Suppliers.Queries.GetSuppliers;

public class GetSuppliersQuery : IRequest<Result<List<SupplierDto>>>
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
}

