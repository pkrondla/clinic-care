using HomoeoDesk.Tenant.Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace HomoeoDesk.Tenant.Application.Features.Suppliers.Commands.DeleteSupplier;

public class DeleteSupplierCommand : IRequest<Result<bool>>
{
    [Required]
    public int Id { get; set; }
}

