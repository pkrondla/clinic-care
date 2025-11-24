using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Suppliers.Queries.GetSuppliers;

public class GetSuppliersQuery : IRequest<Result<List<SupplierDto>>>
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
}

