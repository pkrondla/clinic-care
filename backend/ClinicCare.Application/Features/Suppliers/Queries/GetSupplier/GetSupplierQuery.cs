using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Suppliers.Queries.GetSuppliers;
using MediatR;

namespace ClinicCare.Application.Features.Suppliers.Queries.GetSupplier;

public class GetSupplierQuery : IRequest<Result<SupplierDto>>
{
    public int Id { get; set; }
}

