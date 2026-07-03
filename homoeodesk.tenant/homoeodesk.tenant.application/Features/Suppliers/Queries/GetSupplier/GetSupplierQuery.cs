using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Suppliers.Queries.GetSuppliers;
using MediatR;

namespace HomoeoDesk.Tenant.Application.Features.Suppliers.Queries.GetSupplier;

public class GetSupplierQuery : IRequest<Result<SupplierDto>>
{
    public int Id { get; set; }
}

